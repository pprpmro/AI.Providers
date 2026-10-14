namespace AI.Providers.Configuration.StaticName;

public static class ChatGPTModels
{
	public const string GPT_5_2 = "gpt-5.2";
	public const string GPT_5_2_Chat = "gpt-5.2-chat";
	public const string GPT_5_Mini = "gpt-5-mini";
	public const string GPT_5_Nano = "gpt-5-nano";

	public const string GPT_4_1 = "gpt-4.1";
	public const string GPT_4_1_Mini = "gpt-4.1-mini";
	public const string GPT_4_1_Nano = "gpt-4.1-nano";
	public const string GPT_4o_Mini = "gpt-4o-mini";

	public const string GPT_3_5_Turbo = "gpt-3.5-turbo";

	public static readonly string[] AllDialog = new[]
	{
		GPT_5_2,
		GPT_5_2_Chat,
		GPT_5_Mini,
		GPT_5_Nano,
		GPT_4_1,
		GPT_4_1_Mini,
		GPT_4_1_Nano,
		GPT_4o_Mini,
		GPT_3_5_Turbo
	};

	public static class Embeddings
	{
		public const string Small = "text-embedding-3-small";
		public const string Large = "text-embedding-3-large";
		public const string Ada = "text-embedding-ada-002";

		public static readonly string[] All = [Small, Large, Ada];
	}
}