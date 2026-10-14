using AI.Providers.Configuration.Interfaces;

namespace AI.Providers.Configuration.Implementations;

public class AccessorConfigurationProvider : IConfigurationProvider
{
	private readonly Func<string, string?> accessor;

	public AccessorConfigurationProvider(Func<string, string?> accessor)
	{
		this.accessor = accessor;
	}

	public string? Get(string key)
	{
		ArgumentNullException.ThrowIfNull(key);
		return accessor(key);
	}
}