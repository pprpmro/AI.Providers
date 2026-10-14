using AI.Providers.Configuration;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Data.Enumerations;
using AI.Providers.Data.Vectorizers;

namespace AI.Providers.Configuration.Providers;

public static class ChatGPTConfiguration
{
	public static string ApiKey(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.ChatGPT.ApiKey);

	public static string DialogEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.ChatGPT.DialogEndpoint, Defaults.ChatGPT.DialogEndpoint);

	public static string EmbeddingEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.ChatGPT.EmbeddingEndpoint, Defaults.ChatGPT.EmbeddingEndpoint);

	public static string ModelsEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.ChatGPT.ModelsEndpoint, Defaults.ChatGPT.ModelsEndpoint);

	public static string DialogModel(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.ChatGPT.DialogModel, Defaults.ChatGPT.DialogModel);

	public static Embedder EmbeddingModel(IConfigurationProvider cfg)
	{
		var model = cfg.GetOrDefault(ConfigurationKeys.ChatGPT.EmbeddingModel, Defaults.ChatGPT.EmbeddingModel);
		var length = model switch
		{
			"text-embedding-3-large" => 3072,
			"text-embedding-ada-002" => 1536,
			_ => 1536
		};

		return new Embedder(model, length, ProviderType.ChatGPT);
	}
}
