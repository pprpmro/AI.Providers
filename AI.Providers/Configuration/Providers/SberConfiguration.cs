using AI.Providers.Configuration;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Data.Enumerations;
using AI.Providers.Data.Vectorizers;

namespace AI.Providers.Configuration.Providers;

public static class SberConfiguration
{
	public static string AuthorizeData(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.Sber.AuthorizeData);

	public static string SaluteSpeechCredentials(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.Sber.SaluteSpeechCredentials);

	public static string AccessTokenEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.AccessTokenEndpoint, Defaults.Sber.AccessTokenEndpoint);

	public static string DialogEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.DialogEndpoint, Defaults.Sber.DialogEndpoint);

	public static string EmbeddingEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.EmbeddingEndpoint, Defaults.Sber.EmbeddingEndpoint);

	public static string ModelsEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.ModelsEndpoint, Defaults.Sber.ModelsEndpoint);

	public static string DialogModel(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.DialogModel, Defaults.Sber.DialogModel);

	public static string DialogScope(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.GigaChatScope, Defaults.Sber.GigaChatScope);

	public static string SaluteSpeechScope(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.Sber.SaluteSpeechScope, Defaults.Sber.SaluteSpeechScope);

	public static Embedder EmbeddingModel(IConfigurationProvider cfg)
	{
		var model = cfg.GetOrDefault(ConfigurationKeys.Sber.EmbeddingModel, Defaults.Sber.EmbeddingModel);

		var length = model switch
		{
			"EmbeddingsGigaR" => 4096,
			_ => 512
		};

		return new Embedder(model, length, ProviderType.GigaChat);
	}
}
