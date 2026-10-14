using System.Net.Http.Headers;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Implementations
{
	public class OpenAiCompatibleDialog : BaseChatDialog<Message, Request>
	{
		public OpenAiCompatibleDialog(string modelName, string completionsEndpoint)
			: base(modelName, completionsEndpoint) { }

		public OpenAiCompatibleDialog(string modelName, string completionsEndpoint, string apiKey)
			: base(modelName, completionsEndpoint)
		{
			AddApiKey(apiKey);
		}

		public OpenAiCompatibleDialog(string modelName, string completionsEndpoint, HttpClient client)
			: base(modelName, completionsEndpoint, client) { }

		public OpenAiCompatibleDialog(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public OpenAiCompatibleDialog(IConfigurationProvider cfg, HttpClient? client)
			: base(DefaultGPTConfiguration.DialogModel(cfg), DefaultGPTConfiguration.DialogEndpoint(cfg), client)
		{
			AddApiKey(DefaultGPTConfiguration.ApiKey(cfg));
		}

		private void AddApiKey(string apiKey)
		{
			if (!string.IsNullOrWhiteSpace(apiKey))
			{
				authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			}
		}
	}
}
