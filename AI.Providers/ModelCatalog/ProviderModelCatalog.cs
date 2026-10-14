using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog
{
	public sealed record ProviderModelCatalog(
		ProviderType Provider,
		IReadOnlyList<ProviderModel> Models,
		bool IsAvailable,
		string? ErrorMessage = null);
}
