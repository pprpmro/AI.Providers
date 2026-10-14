namespace AI.Providers.Configuration;

public static class Defaults
{
	public static class ChatGPT
	{
		public const string DialogEndpoint = "https://api.openai.com/v1/chat/completions";
		public const string EmbeddingEndpoint = "https://api.openai.com/v1/embeddings";
		public const string ModelsEndpoint = "https://api.openai.com/v1/models";
		public const string DialogModel = "gpt-4o-mini";
		public const string EmbeddingModel = "text-embedding-3-small";
	}

	public static class YandexGPT
	{
		public const string DialogEndpoint = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";
		public const string OpenAiLikeEndpoint = "https://ai.api.cloud.yandex.net/v1/chat/completions";
		public const string ModelsEndpoint = "https://ai.api.cloud.yandex.net/v1/models";
		public const string EmbeddingEndpoint = "https://ai.api.cloud.yandex.net/v1/embeddings";
	}

	public static class Sber
	{
		public const string GigaChatScope = "GIGACHAT_API_PERS";
		public const string SaluteSpeechScope = "SALUTE_SPEECH_PERS";

		public const string AccessTokenEndpoint = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
		public const string DialogEndpoint = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";
		public const string EmbeddingEndpoint = "https://gigachat.devices.sberbank.ru/api/v1/embeddings";
		public const string ModelsEndpoint = "https://api.giga.chat/v1/models";

		public const string DialogModel = "GigaChat-2-Pro";
		public const string EmbeddingModel = "Embeddings";
	}

	public static class DeepSeek
	{
		public const string DialogEndpoint = "https://api.deepseek.com/chat/completions";
		public const string ModelsEndpoint = "https://api.deepseek.com/models";
		public const string DialogModel = "deepseek-chat";
	}
	public static class DefaultGPT
	{
		public const string DialogEndpoint = "http://localhost:1234/v1/chat/completions";
		public const string ModelsEndpoint = "http://localhost:1234/v1/models";
		public const string DialogModel = "local-model";
	}
}
