using AI.Providers.Data.Vectorizers;

namespace AI.Providers.Vectorizers.Interfaces
{
	public interface IVectorizer : IDisposable
	{
		Task<VectorizerResponse> GetEmbeddingAsync(VectorizerRequest request, CancellationToken cancellationToken = default);
		void SetModel(string model);
	}
}
