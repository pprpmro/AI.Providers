using System.Net.Http.Headers;
using AI.Providers.Common;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Enumerations;

namespace AI.Providers.ModelCatalog.Implementations
{
	public class GigaChatModelCatalogProvider : ModelCatalogProviderBase
	{
		private readonly SberAccessTokenProvider accessTokenProvider;

		public GigaChatModelCatalogProvider(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public GigaChatModelCatalogProvider(IConfigurationProvider cfg, HttpClient? client)
			: base(ProviderType.GigaChat, SberConfiguration.ModelsEndpoint(cfg), client)
		{
			accessTokenProvider = new SberAccessTokenProvider(
				httpClient,
				SberConfiguration.AccessTokenEndpoint(cfg),
				SberConfiguration.DialogScope(cfg),
				SberConfiguration.AuthorizeData(cfg));
		}

		protected override void ConfigureRequest(HttpRequestMessage request)
		{
			base.ConfigureRequest(request);
			request.Headers.Accept.ParseAdd("application/json");
		}

		public override async Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default)
		{
			authorization = new AuthenticationHeaderValue("Bearer", await accessTokenProvider.GetAccessTokenAsync(cancellationToken));
			return await base.GetModelsAsync(cancellationToken);
		}
	}
}
