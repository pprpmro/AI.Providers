using AI.Providers.Data.Vectorizers;
using AI.Providers.Vectorizers.Interfaces;

namespace AI.Providers.Factories.Interfaces
{
	/// <summary>Creates a vectorizer for the provider and model in an embedding configuration.</summary>
	public interface IVectorizerFactory
	{
		/// <summary>Creates a vectorizer with its own HTTP client. The caller disposes the vectorizer.</summary>
		IVectorizer Create(Embedder embedder);

		/// <summary>Creates a vectorizer that can reuse a caller-owned HTTP client.</summary>
		IVectorizer Create(Embedder embedder, HttpClient? client);
	}
}
