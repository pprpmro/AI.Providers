using System.Net.Http.Headers;
using System.Net.Http.Json;
using AI.Providers.Configuration.Interfaces;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Vectorizers;
using AI.Providers.Vectorizers.Interfaces;

namespace AI.Providers.Vectorizers.Implementation
{
	public class YandexGPTVectorizer : IVectorizer, IDisposable
	{
		private string currentModelPath;
		private readonly string embeddingEndpoint;
		private readonly string catalogId;
		private readonly HttpClient httpClient;
		private readonly AuthenticationHeaderValue authorization;
		private readonly bool ownsHttpClient;

		public YandexGPTVectorizer(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public YandexGPTVectorizer(IConfigurationProvider cfg, HttpClient? client)
		{
			var apiKey = YandexGPTConfiguration.ApiKey(cfg);
			catalogId = YandexGPTConfiguration.CatalogId(cfg);
			httpClient = client ?? new HttpClient();
			ownsHttpClient = client is null;
			authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			currentModelPath = YandexGPTConfiguration.EmbedderSmallTexts(cfg).Model;
			embeddingEndpoint = YandexGPTConfiguration.EmbeddingEndpoint(cfg);
		}

		public async Task<VectorizerResponse> GetEmbeddingAsync(VectorizerRequest request, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);
			cancellationToken.ThrowIfCancellationRequested();

			var input = request.GetInputs();
			if (input.Length == 0)
				return new VectorizerResponse();

			var model = request.GetModel(currentModelPath);
			var encodingFormat = request.GetEncodingFormat();
			var embeddings = new float[input.Length][];

			// Yandex accepts one text per request.
			for (var i = 0; i < input.Length; i++)
			{
				using var httpRequest = new HttpRequestMessage(HttpMethod.Post, embeddingEndpoint)
				{
					Content = JsonContent.Create(new { model, input = input[i], encoding_format = encodingFormat })
				};
				httpRequest.Headers.Authorization = authorization;
				if (!string.IsNullOrWhiteSpace(catalogId))
					httpRequest.Headers.Add("OpenAI-Project", catalogId);

				using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
				var result = await EmbeddingResponseReader.ReadAsync(response, 1, cancellationToken);
				embeddings[i] = result.Embedding[0];
			}

			return new VectorizerResponse { Embedding = embeddings };
		}

		public void Dispose()
		{
			if (ownsHttpClient)
				httpClient.Dispose();
		}

		public void SetModel(string model)
		{
			currentModelPath = model;
		}
	}
}
