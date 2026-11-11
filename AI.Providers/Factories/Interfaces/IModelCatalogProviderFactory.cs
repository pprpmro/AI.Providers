using AI.Providers.Data.Enumerations;
using AI.Providers.ModelCatalog.Interfaces;

namespace AI.Providers.Factories.Interfaces
{
	/// <summary>Creates adapters for provider model listing endpoints.</summary>
	public interface IModelCatalogProviderFactory
	{
		/// <summary>Creates a model catalog adapter with its own HTTP client.</summary>
		/// <param name="providerType">The provider to use.</param>
		/// <returns>A new adapter that the caller must dispose.</returns>
		IModelCatalogProvider Create(ProviderType providerType);
		/// <summary>Creates a model catalog adapter that can reuse a caller-supplied HTTP client.</summary>
		/// <param name="providerType">The provider to use.</param>
		/// <param name="client">An optional HTTP client owned by the caller. Pass <see langword="null"/> to create a client for this instance.</param>
		/// <returns>A new adapter that the caller must dispose.</returns>
		IModelCatalogProvider Create(ProviderType providerType, HttpClient? client);
	}
}
