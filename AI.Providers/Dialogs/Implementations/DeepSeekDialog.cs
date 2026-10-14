using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Implementations
{
	public class DeepSeekDialog : BaseChatDialog<Message, Request>
	{
		public DeepSeekDialog(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public DeepSeekDialog(IConfigurationProvider cfg, HttpClient? client)
			: base(DeepSeekConfiguration.DialogModel(cfg), DeepSeekConfiguration.DialogEndpoint(cfg), client)
		{
			authorization = new AuthenticationHeaderValue("Bearer", DeepSeekConfiguration.ApiKey(cfg));
		}
	}
}
