using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Implementations
{
	public class DeepSeekModelCatalogProvider : ModelCatalogProviderBase
	{
		public DeepSeekModelCatalogProvider(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public DeepSeekModelCatalogProvider(IConfigurationProvider cfg, HttpClient? client)
			: base(ProviderType.DeepSeek, DeepSeekConfiguration.ModelsEndpoint(cfg), client)
		{
			var apiKey = DeepSeekConfiguration.ApiKey(cfg);
			if (!string.IsNullOrWhiteSpace(apiKey))
			{
				authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			}
		}
	}
}