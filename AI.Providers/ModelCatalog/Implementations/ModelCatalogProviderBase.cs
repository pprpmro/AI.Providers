using System.Net.Http.Headers;
using System.Text.Json;
using AI.Providers.Data.Enumerations;
using AI.Providers.ModelCatalog.Interfaces;

namespace AI.Providers.ModelCatalog.Implementations
{
	public abstract class ModelCatalogProviderBase : IModelCatalogProvider, IDisposable
	{
		protected readonly HttpClient httpClient;
		protected AuthenticationHeaderValue? authorization;
		private readonly bool ownsHttpClient;
		protected readonly string modelsEndpoint;

		protected ModelCatalogProviderBase(ProviderType provider, string modelsEndpoint)
			: this(provider, modelsEndpoint, null) { }

		protected ModelCatalogProviderBase(ProviderType provider, string modelsEndpoint, HttpClient? client)
		{
			Provider = provider;
			this.modelsEndpoint = string.IsNullOrWhiteSpace(modelsEndpoint)
				? throw new ArgumentException("Models endpoint cannot be empty.", nameof(modelsEndpoint))
				: modelsEndpoint;
			httpClient = client ?? new HttpClient();
			ownsHttpClient = client is null;
		}

		public void Dispose()
		{
			if (ownsHttpClient)
				httpClient.Dispose();
		}

		public ProviderType Provider { get; }

		public virtual async Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, modelsEndpoint);
			ConfigureRequest(request);
			using var response = await httpClient.SendAsync(
				request,
				HttpCompletionOption.ResponseContentRead,
				cancellationToken);

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			if (!response.IsSuccessStatusCode)
				throw new InvalidOperationException($"{Provider} models request failed: {(int)response.StatusCode} {response.StatusCode}. Body: {content}");

			return ParseModels(content);
		}

		protected virtual void ConfigureRequest(HttpRequestMessage request)
		{
			request.Headers.Authorization = authorization;
		}

		protected virtual IReadOnlyList<ProviderModel> ParseModels(string json)
		{
			using var document = JsonDocument.Parse(json);
			return ParseOpenAiCompatibleModels(document);
		}

		protected IReadOnlyList<ProviderModel> ParseOpenAiCompatibleModels(JsonDocument document)
		{
			if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
				return Array.Empty<ProviderModel>();

			var models = new List<ProviderModel>();
			foreach (var modelElement in data.EnumerateArray())
			{
				var id = GetString(modelElement, "id") ?? GetString(modelElement, "name");
				if (string.IsNullOrWhiteSpace(id))
					continue;

				models.Add(new ProviderModel(
					Provider,
					id,
					GetString(modelElement, "name") ?? id,
					GetString(modelElement, "owned_by"),
					GetString(modelElement, "type")));
			}

			return models;
		}

		private static string? GetString(JsonElement element, string propertyName)
		{
			return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
				? property.GetString()
				: null;
		}
	}
}
