using AI.Providers.Data.Enumerations;
using AI.Providers.ModelCatalog.Interfaces;

namespace AI.Providers.Factories.Interfaces
{
	public interface IModelCatalogProviderFactory
	{
		IModelCatalogProvider Create(ProviderType providerType);
		IModelCatalogProvider Create(ProviderType providerType, HttpClient? client);
	}
}
