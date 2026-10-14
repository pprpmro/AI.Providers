using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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
			cancellationToken.ThrowIfCancellationRequested();

			request.Model = currentModel;

			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, embeddingEndpoint)
			{
				Content = JsonContent.Create(request)
			};
			httpRequest.Headers.Authorization = authorization;
			using var response = await httpClient.SendAsync(httpRequest, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"{(int)response.StatusCode} {response.StatusCode}");
			}

			var node = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))!;
			var embeddings = node["data"]!
				.AsArray()
				.Select(item => item!["embedding"]!
								.AsArray()
								.Select(x => (float)x!.GetValue<double>())
								.ToArray())
				.ToArray();

			cancellationToken.ThrowIfCancellationRequested();
			return new VectorizerResponse { Embedding = embeddings };
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
