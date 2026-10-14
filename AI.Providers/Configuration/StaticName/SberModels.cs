namespace AI.Providers.Configuration.StaticName;

public static class SberModels
{
	public const string Pro = "GigaChat-Pro";
	public const string Pro_preview = "GigaChat-Pro-preview";
	public const string Pro2 = "GigaChat-2-Pro";

	public const string Max = "GigaChat-Max";
	public const string Max_preview = "GigaChat-Max-preview";
	public const string Max2 = "GigaChat-2-Max";

	public const string Lite = "GigaChat";
	public const string Lite_preview = "GigaChat-preview";
	public const string Lite2 = "GigaChat-2";

	public static readonly string[] Dialog =
	[
		Pro2, Max2, Lite2,
		Pro, Max, Lite,
		Pro_preview, Max_preview, Lite_preview
	];

	public static class Embeddings
	{
		public const string Default = "Embeddings";
		public const string Giga = "EmbeddingsGigaR";
		public static readonly string[] All = [Default, Giga];
	}
}