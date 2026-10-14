using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs;
using AI.Providers.Dialogs.Implementations;
using AI.Providers.Factories.Implementations;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class YandexGPTDialogTests
{
	[Theory]
	[InlineData(null, false)]
	[InlineData("https://proxy.example.test/v1/chat/completions", true)]
	public async Task UsesOpenAiCompatibleEndpointAndProtocol(string? endpoint, bool stream)
	{
		var settings = new Dictionary<string, string>
		{
			[ConfigurationKeys.YandexGPT.ApiKey] = "test-api-key",
			[ConfigurationKeys.YandexGPT.CatalogId] = "test-folder",
			[ConfigurationKeys.YandexGPT.DialogEndpoint] = "https://native.example.test/foundationModels/v1/completion"
		};
		if (endpoint is not null)
			settings[ConfigurationKeys.YandexGPT.OpenAiLikeEndpoint] = endpoint;

		var configuration = new DictionaryAccessorConfigurationProvider(() => settings);
		var requestCount = 0;
		using var client = new HttpClient(new MockHttpMessageHandler(async (request, ct) =>
		{
			requestCount++;
			Assert.Equal(HttpMethod.Post, request.Method);
			Assert.Equal(endpoint ?? "https://ai.api.cloud.yandex.net/v1/chat/completions", request.RequestUri!.AbsoluteUri);
			Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
			Assert.Equal("test-api-key", request.Headers.Authorization.Parameter);

			using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
			Assert.Equal("gpt://test-folder/yandexgpt-lite/latest", body.RootElement.GetProperty("model").GetString());
			Assert.Equal(stream, body.RootElement.GetProperty("stream").GetBoolean());
			Assert.Equal("user", body.RootElement.GetProperty("messages")[0].GetProperty("role").GetString());
			Assert.Equal("hello", body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
			Assert.False(body.RootElement.TryGetProperty("modelUri", out _));

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = stream
					? new StringContent(
						"data: {\"choices\":[{\"delta\":{\"content\":\"reply\"}}]}\n\ndata: [DONE]\n\n",
						Encoding.UTF8, "text/event-stream")
					: JsonContent.Create(new
					{
						choices = new[] { new { message = new { role = "assistant", content = "reply" } } }
					})
			};
		}));
		using var dialog = (YandexGPTDialog)new DialogFactory(configuration).Create(ProviderType.YandexGPT, client);
		var chunks = new List<string>();

		var response = await dialog.SendMessage(new ChatRequest { Message = "hello", Stream = stream }, chunks.Add);

		Assert.Equal("reply", response);
		Assert.Equal(1, requestCount);
		Assert.Equal(stream ? new[] { "reply" } : Array.Empty<string>(), chunks);
		Assert.Equal(2, dialog.MessageCount);
		Assert.Equal("reply", dialog.GetHistoryCopy()[1].Content);
		Assert.Null(client.DefaultRequestHeaders.Authorization);
	}
}
