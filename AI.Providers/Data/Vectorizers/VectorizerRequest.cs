using System.Text.Json.Serialization;

namespace AI.Providers.Data.Vectorizers
{
	public class VectorizerRequest : IVectorizerRequest
	{
		[JsonPropertyName("input")]
		public string[] Input { get; set; } = Array.Empty<string>();

		[JsonPropertyName("model")]
		public string Model { get; set; } = "";

		[JsonPropertyName("encoding_format")] 
		public string Encoding_format { get; set; } = "";
	}
}
