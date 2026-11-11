using System.Net.Http.Headers;
using System.Net.Http.Json;
using AI.Providers.Common;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Vectorizers;
using AI.Providers.Vectorizers.Interfaces;

namespace AI.Providers.Vectorizers.Implementation
{
	public class GigaChatVectorizer : IVectorizer, IDisposable
	{
		private string currentModel;

		private readonly SberAccessTokenProvider authentificator;
		private readonly HttpClient httpClient;
		private readonly bool ownsHttpClient;

		private readonly string embeddingEndpoint;

		public GigaChatVectorizer(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public GigaChatVectorizer(IConfigurationProvider cfg, HttpClient? client)
		{
			embeddingEndpoint = SberConfiguration.EmbeddingEndpoint(cfg);
			currentModel = SberConfiguration.EmbeddingModel(cfg).Model;

			httpClient = client ?? new HttpClient();
			ownsHttpClient = client is null;
			authentificator = new SberAccessTokenProvider(
				httpClient,
				SberConfiguration.AccessTokenEndpoint(cfg),
				SberConfiguration.DialogScope(cfg),
				SberConfiguration.AuthorizeData(cfg)
			);
		}

		public async Task<VectorizerResponse> GetEmbeddingAsync(VectorizerRequest request, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);
			cancellationToken.ThrowIfCancellationRequested();

			var input = request.GetInputs();
			if (input.Length == 0)
				return new VectorizerResponse();

			request.GetEncodingFormat();
			var model = request.GetModel(currentModel);
			var accessToken = await authentificator.GetAccessTokenAsync(cancellationToken);

			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, embeddingEndpoint)
			{
				Content = JsonContent.Create(new { model, input })
			};
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
			return await EmbeddingResponseReader.ReadAsync(response, input.Length, cancellationToken);
		}

		// The caller retains ownership of an injected client.
		public void Dispose()
		{
			if (ownsHttpClient)
				httpClient.Dispose();
		}

		public void SetModel(string model)
		{
			currentModel = model;
		}
	}
}
