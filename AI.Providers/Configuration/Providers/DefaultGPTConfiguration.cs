using AI.Providers.Configuration;
using AI.Providers.Configuration.Interfaces;

namespace AI.Providers.Configuration.Providers;

public static class DefaultGPTConfiguration
{
	public static string ApiKey(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.DefaultGPT.ApiKey);

	public static string DialogEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.DefaultGPT.DialogEndpoint, Defaults.DefaultGPT.DialogEndpoint);

	public static string ModelsEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.DefaultGPT.ModelsEndpoint, Defaults.DefaultGPT.ModelsEndpoint);

	public static string DialogModel(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.DefaultGPT.DialogModel, Defaults.DefaultGPT.DialogModel);
}
