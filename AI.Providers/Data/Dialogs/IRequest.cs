using System.Text.Json.Serialization;

namespace AI.Providers.Data.Dialogs
{
	public interface IRequest
	{
		int AnswerCount { get; set; }
		IReadOnlyList<IMessage> Messages { get; set; }
		string Model { get; set; }
		double? Temperature { get; set; }
		bool Stream {  get; set; }
	}

	public class Request : IRequest
	{
		[JsonPropertyName("model")]
		public string Model { get; set; } = "";

		[JsonPropertyName("messages")]
		public IReadOnlyList<IMessage> Messages { get; set; }  = Array.Empty<IMessage>();

		[JsonPropertyName("n")]
		public int AnswerCount { get; set; } = 1;

		[JsonPropertyName("temperature")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public double? Temperature { get; set; }

		[JsonPropertyName("stream")]
		public bool Stream { get; set; } = false;
	}
}