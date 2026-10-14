using AI.Providers.Configuration.Interfaces;
using AI.Providers.Data.Enumerations;
using AI.Providers.Data.Vectorizers;
using AI.Providers.Factories.Interfaces;
using AI.Providers.Vectorizers.Implementation;
using AI.Providers.Vectorizers.Interfaces;

namespace AI.Providers.Factories.Implementations
{
	public class VectorizerFactory : IVectorizerFactory
	{
		private readonly IConfigurationProvider configurationProvider;
		public VectorizerFactory(IConfigurationProvider configurationProvider)
		{
			this.configurationProvider = configurationProvider;
		}
		public IVectorizer Create(Embedder embedder)
			=> Create(embedder, null);

		public IVectorizer Create(Embedder embedder, HttpClient? client)
		{
			IVectorizer vectorizer = embedder.ProviderType switch
			{
				ProviderType.ChatGPT => new ChatGPTVectorizer(configurationProvider, client),
				ProviderType.YandexGPT => new YandexGPTVectorizer(configurationProvider, client),
				ProviderType.GigaChat => new GigaChatVectorizer(configurationProvider, client),
				_ => throw new NotSupportedException()
			};

			vectorizer.SetModel(embedder.Model);
			return vectorizer;
		}
	}
}
