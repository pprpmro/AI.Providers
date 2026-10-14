using AI.Providers.Data.Enumerations;

namespace AI.Providers.Configuration
{
	public static class ConfigurationDefaultsSeeder
	{
		public static bool EnsureDefaults(ProviderType provider, IDictionary<string, string> cfg) => provider switch
		{
			ProviderType.ChatGPT => EnsureChatGpt(cfg),
			ProviderType.YandexGPT => EnsureYandex(cfg),
			ProviderType.GigaChat => EnsureSber(cfg),
			ProviderType.DeepSeek => EnsureDeepSeek(cfg),
			ProviderType.DefaultDialog => EnsureDefaultDialog(cfg),
			_ => false
		};

		private static bool EnsureChatGpt(IDictionary<string, string> cfg)
		{
			var changed = false;

			changed |= SetIfMissing(cfg, ConfigurationKeys.ChatGPT.ApiKey, "");

			changed |= SetIfMissing(cfg, ConfigurationKeys.ChatGPT.DialogEndpoint, Defaults.ChatGPT.DialogEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.ChatGPT.EmbeddingEndpoint, Defaults.ChatGPT.EmbeddingEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.ChatGPT.ModelsEndpoint, Defaults.ChatGPT.ModelsEndpoint);

			return changed;
		}

		private static bool EnsureYandex(IDictionary<string, string> cfg)
		{
			var changed = false;

			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.ApiKey, "");
			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.CatalogId, "");
			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.KeyId, "");

			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.DialogEndpoint, Defaults.YandexGPT.DialogEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.OpenAiLikeEndpoint, Defaults.YandexGPT.OpenAiLikeEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.ModelsEndpoint, Defaults.YandexGPT.ModelsEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.YandexGPT.EmbeddingEndpoint, Defaults.YandexGPT.EmbeddingEndpoint);

			return changed;
		}

		private static bool EnsureSber(IDictionary<string, string> cfg)
		{
			var changed = false;

			changed |= SetIfMissing(cfg, ConfigurationKeys.Sber.AuthorizeData, "");
			changed |= SetIfMissing(cfg, ConfigurationKeys.Sber.SaluteSpeechCredentials, "");

			changed |= SetIfMissing(cfg, ConfigurationKeys.Sber.DialogEndpoint, Defaults.Sber.DialogEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.Sber.EmbeddingEndpoint, Defaults.Sber.EmbeddingEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.Sber.ModelsEndpoint, Defaults.Sber.ModelsEndpoint);

			return changed;
		}

		private static bool EnsureDeepSeek(IDictionary<string, string> cfg)
		{
			var changed = false;

			changed |= SetIfMissing(cfg, ConfigurationKeys.DeepSeek.ApiKey, "");
			changed |= SetIfMissing(cfg, ConfigurationKeys.DeepSeek.DialogEndpoint, Defaults.DeepSeek.DialogEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.DeepSeek.ModelsEndpoint, Defaults.DeepSeek.ModelsEndpoint);

			return changed;
		}

		private static bool EnsureDefaultDialog(IDictionary<string, string> cfg)
		{
			var changed = false;

			changed |= SetIfMissing(cfg, ConfigurationKeys.DefaultGPT.ApiKey, "");
			changed |= SetIfMissing(cfg, ConfigurationKeys.DefaultGPT.DialogEndpoint, Defaults.DefaultGPT.DialogEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.DefaultGPT.ModelsEndpoint, Defaults.DefaultGPT.ModelsEndpoint);
			changed |= SetIfMissing(cfg, ConfigurationKeys.DefaultGPT.DialogModel, Defaults.DefaultGPT.DialogModel);

			return changed;
		}

		private static bool SetIfMissing(IDictionary<string, string> cfg, string key, string value)
		{
			if (cfg.ContainsKey(key))
				return false;
			cfg[key] = value;
			return true;
		}
	}
}
