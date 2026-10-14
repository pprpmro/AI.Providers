namespace AI.Providers.Data.Vectorizers
{
	public interface IVectorizerRequest
	{
		string[] Input { get; set; }
		string Model { get; set; }
		string Encoding_format { get; set; }
	}
}
