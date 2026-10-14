using AI.Providers.Data.Vectorizers;
using AI.Providers.Vectorizers.Interfaces;

namespace AI.Providers.Factories.Interfaces
{
	public interface IVectorizerFactory
	{
		IVectorizer Create(Embedder embedder);
		IVectorizer Create(Embedder embedder, HttpClient? client);
	}
}
