using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Implementations
{
	public class YandexGPTModelCatalogProvider : ModelCatalogProviderBase
	{
		private readonly string catalogId;

		public YandexGPTModelCatalogProvider(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public YandexGPTModelCatalogProvider(IConfigurationProvider cfg, HttpClient? client)
			: base(ProviderType.YandexGPT, YandexGPTConfiguration.ModelsEndpoint(cfg), client)
		{
			var apiKey = YandexGPTConfiguration.ApiKey(cfg);
			if (!string.IsNullOrWhiteSpace(apiKey))
			{
				authorization = new AuthenticationHeaderValue("Api-Key", apiKey);
			}

			catalogId = YandexGPTConfiguration.CatalogId(cfg);
		}

		protected override void ConfigureRequest(HttpRequestMessage request)
		{
			base.ConfigureRequest(request);
			if (!string.IsNullOrWhiteSpace(catalogId))
				request.Headers.Add("x-project", catalogId);
		}
	}
}