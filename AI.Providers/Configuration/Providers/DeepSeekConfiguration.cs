using AI.Providers.Configuration;
using AI.Providers.Configuration.Interfaces;

namespace AI.Providers.Configuration.Providers;

public static class DeepSeekConfiguration
{
	public static string ApiKey(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.DeepSeek.ApiKey);

	public static string DialogEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.DeepSeek.DialogEndpoint, Defaults.DeepSeek.DialogEndpoint);

	public static string ModelsEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.DeepSeek.ModelsEndpoint, Defaults.DeepSeek.ModelsEndpoint);

	public static string DialogModel(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.DeepSeek.DialogModel, Defaults.DeepSeek.DialogModel);
}
