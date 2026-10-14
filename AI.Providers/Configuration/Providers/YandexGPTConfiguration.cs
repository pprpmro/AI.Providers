using AI.Providers.Configuration;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Data.Enumerations;
using AI.Providers.Data.Vectorizers;

namespace AI.Providers.Configuration.Providers;

public static class YandexGPTConfiguration
{
	public static string ApiKey(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.YandexGPT.ApiKey);

	public static string CatalogId(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.YandexGPT.CatalogId);

	public static string KeyId(IConfigurationProvider cfg)
		=> cfg.GetOrEmpty(ConfigurationKeys.YandexGPT.KeyId);

	public static string DialogEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.YandexGPT.DialogEndpoint, Defaults.YandexGPT.DialogEndpoint);

	public static string OpenAiLikeDialogEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.YandexGPT.OpenAiLikeEndpoint, Defaults.YandexGPT.OpenAiLikeEndpoint);

	public static string EmbeddingEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.YandexGPT.EmbeddingEndpoint, Defaults.YandexGPT.EmbeddingEndpoint);

	public static string ModelsEndpoint(IConfigurationProvider cfg)
		=> cfg.GetOrDefault(ConfigurationKeys.YandexGPT.ModelsEndpoint, Defaults.YandexGPT.ModelsEndpoint);

	public static string DialogModelLite(IConfigurationProvider cfg)
		=> $"gpt://{CatalogId(cfg)}/yandexgpt-lite/latest";

	public static string DialogModelPro(IConfigurationProvider cfg)
		=> $"gpt://{CatalogId(cfg)}/yandexgpt/latest";

	public static Embedder EmbedderBigTexts(IConfigurationProvider cfg)
	=> new($"emb://{CatalogId(cfg)}/text-search-doc/latest", 256, ProviderType.YandexGPT);

	public static Embedder EmbedderSmallTexts(IConfigurationProvider cfg)
		=> new($"emb://{CatalogId(cfg)}/text-search-query/latest", 256, ProviderType.YandexGPT);
}
