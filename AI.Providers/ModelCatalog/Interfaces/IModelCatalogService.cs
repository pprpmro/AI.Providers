using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Interfaces
{
	public interface IModelCatalogService
	{
		Task<ProviderModelCatalog> GetModelsAsync(
			ProviderType provider,
			CancellationToken cancellationToken = default);
	}
}
