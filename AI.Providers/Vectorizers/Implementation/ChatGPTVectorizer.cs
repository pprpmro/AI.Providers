using System.Net.Http.Headers;
using System.Net.Http.Json;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Vectorizers;
using AI.Providers.Vectorizers.Interfaces;

namespace AI.Providers.Vectorizers.Implementation
{
	public class ChatGPTVectorizer : IVectorizer, IDisposable
	{
		private string currentModel;
		private readonly string embeddingEndpoint;
		private readonly HttpClient httpClient;
		private readonly AuthenticationHeaderValue authorization;
		private readonly bool ownsHttpClient;

		public ChatGPTVectorizer(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public ChatGPTVectorizer(IConfigurationProvider cfg, HttpClient? client)
		{
			embeddingEndpoint = ChatGPTConfiguration.EmbeddingEndpoint(cfg);
			var apiKey = ChatGPTConfiguration.ApiKey(cfg);
			httpClient = client ?? new HttpClient();
			ownsHttpClient = client is null;
			authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			currentModel = ChatGPTConfiguration.EmbeddingModel(cfg).Model;
		}

		public async Task<VectorizerResponse> GetEmbeddingAsync(VectorizerRequest request, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);
			cancellationToken.ThrowIfCancellationRequested();

			var input = request.GetInputs();
			if (input.Length == 0)
				return new VectorizerResponse();

			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, embeddingEndpoint)
			{
				Content = JsonContent.Create(new
				{
					model = request.GetModel(currentModel),
					input,
					encoding_format = request.GetEncodingFormat(allowBase64: true)
				})
			};
			httpRequest.Headers.Authorization = authorization;
			using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
			return await EmbeddingResponseReader.ReadAsync(response, input.Length, cancellationToken);
		}

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
