namespace AI.Providers.Configuration.Interfaces;

public static class ConfigurationProviderExtensions
{
	public static string GetRequired(this IConfigurationProvider cfg, string key)
	{
		var value = cfg.Get(key);
		if (string.IsNullOrWhiteSpace(value))
			throw new InvalidOperationException($"Missing configuration value: {key}");

		return value.Trim();
	}

	public static string GetOrEmpty(this IConfigurationProvider cfg, string key)
	{
		var value = cfg.Get(key);
		return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
	}

	public static string GetOrDefault(this IConfigurationProvider cfg, string key, string defaultValue)
	{
		var value = cfg.Get(key);
		return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
	}

	public static int GetOrDefaultInt(this IConfigurationProvider cfg, string key, int defaultValue)
	{
		var value = cfg.Get(key);
		return int.TryParse(value, out var n) ? n : defaultValue;
	}

	public static bool GetOrDefaultBool(this IConfigurationProvider cfg, string key, bool defaultValue)
	{
		var value = cfg.Get(key);
		return bool.TryParse(value, out var b) ? b : defaultValue;
	}
}