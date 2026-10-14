using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog
{
	public sealed record ProviderModel(
		ProviderType Provider,
		string Id,
		string Name,
		string? OwnedBy = null,
		string? Type = null);
}
