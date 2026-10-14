using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Dialogs;
using AI.Providers.Dialogs.Implementations;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class ChatRequestTests
{
	[Fact]
	public async Task GigaChatForwardsRequestOptionsAndStreamCallback()
	{
		var settings = new Dictionary<string, string>
		{
			[ConfigurationKeys.Sber.AccessTokenEndpoint] = "https://auth.example.test/oauth",
			[ConfigurationKeys.Sber.AuthorizeData] = "test-credentials",
			[ConfigurationKeys.Sber.SaluteSpeechCredentials] = "other-salute-credentials",
			[ConfigurationKeys.Sber.DialogEndpoint] = "https://chat.example.test/completions"
		};
		var configuration = new DictionaryAccessorConfigurationProvider(() => settings);
		string? sentModel = null;
		bool? sentStream = null;
		string? sentMessage = null;

		using var client = new HttpClient(new MockHttpMessageHandler(async (request, ct) =>
		{
			if (request.RequestUri!.Host == "auth.example.test")
			{
				Assert.Equal("Basic", request.Headers.Authorization!.Scheme);
				Assert.Equal("test-credentials", request.Headers.Authorization.Parameter);
				Assert.Equal("scope=GIGACHAT_API_PERS", await request.Content!.ReadAsStringAsync(ct));
				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = JsonContent.Create(new
					{
						access_token = "test-access-token",
						expires_at = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeMilliseconds()
					})
				};
			}

			Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
			Assert.Equal("test-access-token", request.Headers.Authorization.Parameter);
			using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
			sentModel = body.RootElement.GetProperty("model").GetString();
			sentStream = body.RootElement.GetProperty("stream").GetBoolean();
			sentMessage = body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString();

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"data: {\"choices\":[{\"delta\":{\"content\":\"reply\"}}]}\n\ndata: [DONE]\n\n",
					Encoding.UTF8,
					"text/event-stream")
			};
		}));

		using var dialog = new GigaChatDialog(configuration, client);
		var streamedChunks = new List<string>();

		var response = await dialog.SendMessage(
			new ChatRequest
			{
				Message = "hello",
				ModelName = "override-model",
				Stream = true,
				RememberMessage = false
			},
			streamedChunks.Add);

		Assert.Equal("reply", response);
		Assert.Equal(new[] { "reply" }, streamedChunks);
		Assert.Equal("override-model", sentModel);
		Assert.True(sentStream);
		Assert.Equal("hello", sentMessage);
		Assert.Equal(0, dialog.MessageCount);
		Assert.Null(client.DefaultRequestHeaders.Authorization);
	}
}
