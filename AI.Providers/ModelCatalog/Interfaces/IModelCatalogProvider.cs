using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Interfaces
{
	/// <summary>Reads model metadata from a provider.</summary>
	public interface IModelCatalogProvider : IDisposable
	{
		/// <summary>Gets the provider served by this adapter.</summary>
		ProviderType Provider { get; }

		/// <summary>Lists models exposed by the configured endpoint.</summary>
		/// <param name="cancellationToken">A token used to cancel the operation.</param>
		/// <returns>The models returned by the provider.</returns>
		Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default);
	}
}
