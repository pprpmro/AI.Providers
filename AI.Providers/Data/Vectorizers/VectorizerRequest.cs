using System.Text.Json.Serialization;

namespace AI.Providers.Data.Vectorizers
{
	/// <summary>Inputs and optional model settings for an embedding request.</summary>
	public class VectorizerRequest : IVectorizerRequest
	{
		/// <summary>The texts to embed, in the order the vectors should be returned.</summary>
		[JsonPropertyName("input")]
		public string[] Input { get; set; } = Array.Empty<string>();

		/// <summary>An optional model override. An empty value uses the vectorizer's default model.</summary>
		[JsonPropertyName("model")]
		public string Model { get; set; } = "";

		/// <summary>The response encoding. All adapters accept float; OpenAI also accepts base64.</summary>
		[JsonPropertyName("encoding_format")]
		public string Encoding_format { get; set; } = "float";

		internal string[] GetInputs()
		{
			var input = Input;
			ArgumentNullException.ThrowIfNull(input, nameof(Input));
			if (Array.Exists(input, string.IsNullOrWhiteSpace))
				throw new ArgumentException("Embedding inputs cannot contain blank text.", nameof(Input));
			return input;
		}

		internal string GetModel(string defaultModel)
			=> string.IsNullOrWhiteSpace(Model) ? defaultModel : Model.Trim();

		internal string GetEncodingFormat(bool allowBase64 = false)
		{
			var format = Encoding_format?.Trim();
			if (string.IsNullOrEmpty(format))
				return "float";
			if (format == "float" || (allowBase64 && format == "base64"))
				return format;

			throw new ArgumentException(allowBase64
				? "The encoding format must be float or base64."
				: "This provider only supports float embeddings.", nameof(Encoding_format));
		}
	}
}
