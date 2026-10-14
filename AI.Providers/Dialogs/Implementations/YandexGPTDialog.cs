using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Implementations
{
	public class YandexGPTDialog : BaseChatDialog<Message, Request>
	{
		public YandexGPTDialog(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public YandexGPTDialog(IConfigurationProvider cfg, HttpClient? client)
			: base(YandexGPTConfiguration.DialogModelLite(cfg), YandexGPTConfiguration.OpenAiLikeDialogEndpoint(cfg), client)
		{
			authorization = new AuthenticationHeaderValue("Bearer", YandexGPTConfiguration.ApiKey(cfg));
		}
	}
}
