using AI.Providers.Data.Vectorizers;

namespace AI.Providers.Vectorizers.Interfaces
{
	/// <summary>Converts text into embedding vectors using an AI provider.</summary>
	public interface IVectorizer : IDisposable
	{
		/// <summary>Returns one numeric vector per input text in the same order.</summary>
		Task<VectorizerResponse> GetEmbeddingAsync(VectorizerRequest request, CancellationToken cancellationToken = default);

		/// <summary>Sets the default model for requests without an explicit model override.</summary>
		void SetModel(string model);
	}
}
