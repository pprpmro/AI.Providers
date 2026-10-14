using System.Text.Json.Serialization;

namespace AI.Providers.Data.Vectorizers
{
	public class VectorizerResponse : IVectorizerResponse
	{
		[JsonPropertyName("embedding")]
		public float[][] Embedding { get; set; } = Array.Empty<float[]>();
	}
}
