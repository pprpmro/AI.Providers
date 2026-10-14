using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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
		private readonly HttpClient httpClient;
		private readonly AuthenticationHeaderValue authorization;
		private readonly bool ownsHttpClient;

		public YandexGPTVectorizer(IConfigurationProvider cfg)
			: this(cfg, null) { }

		public YandexGPTVectorizer(IConfigurationProvider cfg, HttpClient? client)
		{
			var apiKey = YandexGPTConfiguration.ApiKey(cfg);
			httpClient = client ?? new HttpClient();
			ownsHttpClient = client is null;
			authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			currentModelPath = YandexGPTConfiguration.EmbedderSmallTexts(cfg).Model;
			embeddingEndpoint = YandexGPTConfiguration.EmbeddingEndpoint(cfg);
		}

		public async Task<VectorizerResponse> GetEmbeddingAsync(VectorizerRequest request, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			request.Model = currentModelPath;

			var embeddings = new List<float[]>();
			// Yandex's OpenAI-compatible endpoint accepts one text per request.
			foreach (var input in request.Input)
			{
				var payload = new
				{
					model = currentModelPath,
					input,
					encoding_format = string.IsNullOrEmpty(request.Encoding_format) ? "float" : request.Encoding_format
				};
				using var httpRequest = new HttpRequestMessage(HttpMethod.Post, embeddingEndpoint)
				{
					Content = JsonContent.Create(payload)
				};
				httpRequest.Headers.Authorization = authorization;
				using var response = await httpClient.SendAsync(httpRequest, cancellationToken);

				if (!response.IsSuccessStatusCode)
					throw new Exception($"{(int)response.StatusCode} {response.StatusCode}");

				var node = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))!;
				var embedding = node["data"]![0]!["embedding"]!
					.AsArray()
					.Select(x => (float)x!.GetValue<double>())
					.ToArray();
				embeddings.Add(embedding);
			}

			cancellationToken.ThrowIfCancellationRequested();
			return new VectorizerResponse { Embedding = embeddings.ToArray() };
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
