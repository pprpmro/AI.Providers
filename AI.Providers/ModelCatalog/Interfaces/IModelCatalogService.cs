using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Interfaces
{
	/// <summary>Discovers models for use in chat dialogs.</summary>
	public interface IModelCatalogService
	{
		/// <summary>Retrieves the provider's model catalog and applies chat model filters where applicable.</summary>
		/// <param name="provider">The provider to query.</param>
		/// <param name="cancellationToken">A token used to cancel the operation.</param>
		/// <returns>The catalog, or an unavailable result with an error message if the provider request fails.</returns>
		Task<ProviderModelCatalog> GetModelsAsync(
			ProviderType provider,
			CancellationToken cancellationToken = default);
	}
}
