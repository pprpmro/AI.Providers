using System.Net.Http.Headers;
using AI.Providers.Common;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Implementations
{
	public class GigaChatDialog : BaseChatDialog<Message, Request>
	{
		public GigaChatDialog(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public GigaChatDialog(IConfigurationProvider cfg, HttpClient? client)
			: base(SberConfiguration.DialogModel(cfg), SberConfiguration.DialogEndpoint(cfg), client)
		{
			provider = new SberAccessTokenProvider(
				httpClient,
				SberConfiguration.AccessTokenEndpoint(cfg),
				SberConfiguration.DialogScope(cfg),
				SberConfiguration.AuthorizeData(cfg)
			);
		}

		private readonly SberAccessTokenProvider provider;

		public override async Task<string> SendMessage(
			ChatRequest chatRequest,
			Action<string>? onStreamedData = null,
			CancellationToken cancellationToken = default
			)
		{
			ArgumentNullException.ThrowIfNull(chatRequest);
			authorization = new AuthenticationHeaderValue("Bearer", await provider.GetAccessTokenAsync(cancellationToken));
			return await base.SendMessage(chatRequest, onStreamedData, cancellationToken);
		}
	}
}
