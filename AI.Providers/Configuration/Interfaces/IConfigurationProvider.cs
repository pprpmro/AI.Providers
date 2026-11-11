namespace AI.Providers.Configuration.Interfaces;

/// <summary>Provides settings from a source managed by the calling application.</summary>
public interface IConfigurationProvider
{
	/// <summary>Looks up a configuration value.</summary>
	/// <param name="key">The configuration key.</param>
	/// <returns>The value, or <see langword="null"/> if the key is not set.</returns>
	string? Get(string key);
}