using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Implementations
{
	public class ChatGPTDialog : BaseChatDialog<Message, Request>
	{
		public ChatGPTDialog(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public ChatGPTDialog(IConfigurationProvider cfg, HttpClient? client)
			: base (ChatGPTConfiguration.DialogModel(cfg), ChatGPTConfiguration.DialogEndpoint(cfg), client)
		{
			authorization = new AuthenticationHeaderValue("Bearer", ChatGPTConfiguration.ApiKey(cfg));
		}
	}
}
