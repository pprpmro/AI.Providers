using System.Net;
using AI.Providers.Data.Dialogs;
using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs;
using AI.Providers.Dialogs.Interfaces;
using AI.Providers.ModelCatalog.Implementations;
using AI.Providers.ModelCatalog.Interfaces;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class HttpClientOwnershipTests
{
	[Fact]
	public void DisposingThroughInterfacesReleasesInternallyCreatedClients()
	{
		var dialog = new InspectableDialog();
		var catalog = new InspectableCatalog();

		((IChatDialog)dialog).Dispose();
		((IModelCatalogProvider)catalog).Dispose();

		Assert.Throws<ObjectDisposedException>(() => dialog.Client.Timeout = TimeSpan.FromSeconds(30));
		Assert.Throws<ObjectDisposedException>(() => catalog.Client.Timeout = TimeSpan.FromSeconds(30));
	}

	[Fact]
	public async Task DisposingProvidersLeavesSharedInjectedClientUsable()
	{
		using var client = new HttpClient(new MockHttpMessageHandler((_, _) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));

		new InspectableDialog(client).Dispose();
		new InspectableCatalog(client).Dispose();

		using var response = await client.GetAsync("https://example.test/still-usable");
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private sealed class InspectableDialog(HttpClient? client = null)
		: BaseChatDialog<Message, Request>("model", "https://example.test/chat", client)
	{
		public HttpClient Client => httpClient;
	}

	private sealed class InspectableCatalog(HttpClient? client = null)
		: ModelCatalogProviderBase(ProviderType.DefaultDialog, "https://example.test/models", client)
	{
		public HttpClient Client => httpClient;
	}
}