using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Implementations
{
	public class ChatGPTModelCatalogProvider : ModelCatalogProviderBase
	{
		public ChatGPTModelCatalogProvider(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public ChatGPTModelCatalogProvider(IConfigurationProvider cfg, HttpClient? client)
			: base(ProviderType.ChatGPT, ChatGPTConfiguration.ModelsEndpoint(cfg), client)
		{
			var apiKey = ChatGPTConfiguration.ApiKey(cfg);
			if (!string.IsNullOrWhiteSpace(apiKey))
			{
				authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			}
		}
	}
}