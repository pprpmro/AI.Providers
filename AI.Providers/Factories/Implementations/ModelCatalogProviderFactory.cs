using AI.Providers.Configuration.Interfaces;
using AI.Providers.Data.Enumerations;
using AI.Providers.Factories.Interfaces;
using AI.Providers.ModelCatalog.Implementations;
using AI.Providers.ModelCatalog.Interfaces;

namespace AI.Providers.Factories.Implementations
{
	public class ModelCatalogProviderFactory : IModelCatalogProviderFactory
	{
		private readonly IConfigurationProvider configurationProvider;

		public ModelCatalogProviderFactory(IConfigurationProvider configurationProvider)
		{
			this.configurationProvider = configurationProvider;
		}

		public IModelCatalogProvider Create(ProviderType providerType)
			=> Create(providerType, null);

		public IModelCatalogProvider Create(ProviderType providerType, HttpClient? client)
		{
			return providerType switch
			{
				ProviderType.ChatGPT => new ChatGPTModelCatalogProvider(configurationProvider, client),
				ProviderType.GigaChat => new GigaChatModelCatalogProvider(configurationProvider, client),
				ProviderType.YandexGPT => new YandexGPTModelCatalogProvider(configurationProvider, client),
				ProviderType.DeepSeek => new DeepSeekModelCatalogProvider(configurationProvider, client),
				ProviderType.DefaultDialog => new DefaultGPTModelCatalogProvider(configurationProvider, client),
				_ => throw new NotSupportedException($"Model catalog is not supported for provider '{providerType}'.")
			};
		}
	}
}
