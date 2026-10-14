using System.Net;
using System.Text;
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Data.Enumerations;
using AI.Providers.Factories.Implementations;
using AI.Providers.Factories.Interfaces;
using AI.Providers.ModelCatalog;
using AI.Providers.ModelCatalog.Implementations;
using AI.Providers.ModelCatalog.Interfaces;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class ModelCatalogTests
{
	[Fact]
	public async Task LoadsAndFiltersModelsPreservingMetadataAndDisposesProvider()
	{
		var requests = 0;
		using var client = new HttpClient(new MockHttpMessageHandler((request, _) =>
		{
			requests++;
			Assert.Equal(HttpMethod.Get, request.Method);
			Assert.Equal("https://catalog.example.test/models", request.RequestUri!.AbsoluteUri);
			Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
			Assert.Equal("test-key", request.Headers.Authorization.Parameter);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("""
					{"data":[
						{"id":"gpt-test","name":"Test model","owned_by":"test-owner","type":"chat"},
						{"id":"gpt-other"},
						{"name":"gpt-name-only"},
						{"id":""},
						{"id":"text-embedding-test"},
						{"id":"gpt-image-test"}
					]}
					""", Encoding.UTF8, "application/json")
			});
		}));
		var service = CreateService(client, out var provider);

		var catalog = await service.GetModelsAsync(ProviderType.ChatGPT);

		Assert.True(catalog.IsAvailable);
		Assert.Null(catalog.ErrorMessage);
		Assert.Equal(ProviderType.ChatGPT, catalog.Provider);
		Assert.Equal(3, catalog.Models.Count);
		Assert.Equal(new ProviderModel(ProviderType.ChatGPT, "gpt-test", "Test model", "test-owner", "chat"), catalog.Models[0]);
		Assert.Equal("gpt-other", catalog.Models[1].Name);
		Assert.Equal("gpt-name-only", catalog.Models[2].Id);
		Assert.Equal(1, requests);
		Assert.Equal(1, provider.DisposeCalls);
	}

	[Fact]
	public async Task HttpFailureReturnsUnavailableCatalogAndDisposesProvider()
	{
		using var client = new HttpClient(new MockHttpMessageHandler((_, _) => Task.FromResult(
			new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("maintenance") })));
		var service = CreateService(client, out var provider);

		var catalog = await service.GetModelsAsync(ProviderType.ChatGPT);

		Assert.False(catalog.IsAvailable);
		Assert.Empty(catalog.Models);
		Assert.Contains("503", catalog.ErrorMessage!);
		Assert.Contains("maintenance", catalog.ErrorMessage!);
		Assert.Equal(1, provider.DisposeCalls);
	}

	[Fact]
	public async Task CallerCancellationPropagatesAndDisposesProvider()
	{
		using var cancellation = new CancellationTokenSource();
		using var client = new HttpClient(new MockHttpMessageHandler((_, ct) =>
		{
			cancellation.Cancel();
			ct.ThrowIfCancellationRequested();
			throw new InvalidOperationException("Cancellation was not forwarded.");
		}));
		var service = CreateService(client, out var provider);

		var load = service.GetModelsAsync(ProviderType.ChatGPT, cancellation.Token);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => load);
		Assert.True(load.IsCanceled);
		Assert.Equal(1, provider.DisposeCalls);
	}

	[Fact]
	public async Task TransportTimeoutIsReportedAsUnavailableRatherThanCallerCancellation()
	{
		using var client = new HttpClient(new MockHttpMessageHandler((_, _) =>
			throw new TaskCanceledException("transport timeout", new TimeoutException())));
		var service = CreateService(client, out var provider);

		var catalog = await service.GetModelsAsync(ProviderType.ChatGPT);

		Assert.False(catalog.IsAvailable);
		Assert.Empty(catalog.Models);
		Assert.Contains("transport timeout", catalog.ErrorMessage!);
		Assert.Equal(1, provider.DisposeCalls);
	}

	private static ModelCatalogService CreateService(HttpClient client, out TrackedProvider provider)
	{
		var settings = new Dictionary<string, string>
		{
			[ConfigurationKeys.ChatGPT.ModelsEndpoint] = "https://catalog.example.test/models",
			[ConfigurationKeys.ChatGPT.ApiKey] = "test-key"
		};
		var factory = new ModelCatalogProviderFactory(new DictionaryAccessorConfigurationProvider(() => settings));
		provider = new TrackedProvider(factory.Create(ProviderType.ChatGPT, client));
		return new ModelCatalogService(new SingleProviderFactory(provider));
	}

	private sealed class SingleProviderFactory(IModelCatalogProvider provider) : IModelCatalogProviderFactory
	{
		public IModelCatalogProvider Create(ProviderType providerType) => provider;
		public IModelCatalogProvider Create(ProviderType providerType, HttpClient? client) => provider;
	}

	private sealed class TrackedProvider(IModelCatalogProvider inner) : IModelCatalogProvider
	{
		public int DisposeCalls { get; private set; }
		public ProviderType Provider => inner.Provider;
		public Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default)
			=> inner.GetModelsAsync(cancellationToken);

		public void Dispose()
		{
			DisposeCalls++;
			inner.Dispose();
		}
	}
}