namespace AI.Providers.Configuration;

public static class ConfigurationKeys
{
	public static class ChatGPT
	{
		public const string ApiKey = "providers.chatgpt.apiKey";

		public const string DialogEndpoint = "providers.chatgpt.dialog.endpoint";
		public const string EmbeddingEndpoint = "providers.chatgpt.embedding.endpoint";
		public const string ModelsEndpoint = "providers.chatgpt.models.endpoint";

		public const string DialogModel = "providers.chatgpt.dialog.model";
		public const string EmbeddingModel = "providers.chatgpt.embedding.model";
	}

	public static class YandexGPT
	{
		public const string ApiKey = "providers.yandex.apiKey";
		public const string CatalogId = "providers.yandex.catalogId";
		public const string KeyId = "providers.yandex.keyId";

		public const string DialogEndpoint = "providers.yandex.dialog.endpoint";
		public const string OpenAiLikeEndpoint = "providers.yandex.dialog.openaiLike.endpoint";
		public const string EmbeddingEndpoint = "providers.yandex.embedding.endpoint";
		public const string ModelsEndpoint = "providers.yandex.models.endpoint";
	}

	public static class Sber
	{
		public const string AccessTokenEndpoint = "providers.sber.accessTokenEndpoint";

		public const string AuthorizeData = "providers.sber.authorizeData";
		public const string SaluteSpeechCredentials = "providers.sber.saluteSpeechCredentials";

		public const string DialogEndpoint = "providers.sber.dialog.endpoint";
		public const string EmbeddingEndpoint = "providers.sber.embedding.endpoint";
		public const string ModelsEndpoint = "providers.sber.models.endpoint";

		public const string DialogModel = "providers.sber.dialog.model";
		public const string EmbeddingModel = "providers.sber.embedding.model";

		public const string GigaChatScope = "providers.sber.dialog.scope";
		public const string SaluteSpeechScope = "providers.sber.saluteSpeech.scope";
	}

	public static class DeepSeek
	{
		public const string ApiKey = "providers.deepseek.apiKey";

		public const string DialogEndpoint = "providers.deepseek.dialog.endpoint";
		public const string ModelsEndpoint = "providers.deepseek.models.endpoint";
		public const string DialogModel = "providers.deepseek.dialog.model";
	}

	public static class DefaultGPT
	{
		public const string ApiKey = "providers.default.apiKey";
		public const string DialogEndpoint = "providers.default.dialog.endpoint";
		public const string ModelsEndpoint = "providers.default.models.endpoint";
		public const string DialogModel = "providers.default.dialog.model";
	}
}
