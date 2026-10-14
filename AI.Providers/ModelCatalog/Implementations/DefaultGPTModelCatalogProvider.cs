using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Implementations
{
	public class DefaultGPTModelCatalogProvider : ModelCatalogProviderBase
	{
		public DefaultGPTModelCatalogProvider(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public DefaultGPTModelCatalogProvider(IConfigurationProvider cfg, HttpClient? client)
			: base(ProviderType.DefaultDialog, DefaultGPTConfiguration.ModelsEndpoint(cfg), client)
		{
			var apiKey = DefaultGPTConfiguration.ApiKey(cfg);
			if (!string.IsNullOrWhiteSpace(apiKey))
			{
				authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			}
		}
	}
}
