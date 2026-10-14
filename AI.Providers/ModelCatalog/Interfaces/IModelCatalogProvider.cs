using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Interfaces
{
	public interface IModelCatalogProvider : IDisposable
	{
		ProviderType Provider { get; }

		Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default);
	}
}
