using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Data.Dialogs;
using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs;
using AI.Providers.Dialogs.Implementations;
using AI.Providers.Factories.Implementations;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class DialogHistoryTests
{
	[Fact]
	public async Task TemporaryMessagesReachTheApiButDoNotEnterLaterContext()
	{
		var settings = new Dictionary<string, string>
		{
			[ConfigurationKeys.DefaultGPT.DialogEndpoint] = "https://chat.example.test/completions",
			[ConfigurationKeys.DefaultGPT.ApiKey] = "test-key",
			[ConfigurationKeys.DefaultGPT.DialogModel] = "configured-model"
		};
		var requests = new List<string[]>();
		using var client = new HttpClient(new MockHttpMessageHandler(async (request, ct) =>
		{
			Assert.Equal(settings[ConfigurationKeys.DefaultGPT.DialogEndpoint], request.RequestUri!.AbsoluteUri);
			Assert.Equal("test-key", request.Headers.Authorization!.Parameter);
			using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
			Assert.Equal(requests.Count == 1 ? "override-model" : "configured-model", body.RootElement.GetProperty("model").GetString());
			requests.Add(body.RootElement.GetProperty("messages").EnumerateArray()
				.Select(message => $"{message.GetProperty("role").GetString()}:{message.GetProperty("content").GetString()}").ToArray());
			return Reply($"reply-{requests.Count}");
		}));
		using var dialog = new DialogFactory(new DictionaryAccessorConfigurationProvider(() => settings))
			.Create(ProviderType.DefaultDialog, client);
		dialog.SetSystemPrompt("system");

		await dialog.SendMessage(new ChatRequest { Message = "first" });
		await dialog.SendMessage(new ChatRequest { Message = "scratch", ModelName = "override-model", RememberMessage = false });
		await dialog.SendMessage(new ChatRequest { Message = "second" });

		Assert.Equal(3, requests.Count);
		Assert.Equal(new[] { "system:system", "user:first" }, requests[0]);
		Assert.Equal(new[] { "system:system", "user:first", "assistant:reply-1", "user:scratch" }, requests[1]);
		Assert.Equal(new[] { "system:system", "user:first", "assistant:reply-1", "user:second" }, requests[2]);
		Assert.Equal(new[] { "system:system", "user:first", "assistant:reply-1", "user:second", "assistant:reply-3" },
			dialog.GetHistoryCopy().Select(message => $"{message.Role}:{message.Content}"));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task HttpFailurePreservesHistoryAndDoesNotPolluteRetry(bool rememberMessage)
	{
		var requests = 0;
		using var client = new HttpClient(new MockHttpMessageHandler(async (request, ct) =>
		{
			if (++requests == 1)
				return new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("quota exhausted") };

			using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
			Assert.Equal(new[] { "system", "retry" },
				body.RootElement.GetProperty("messages").EnumerateArray().Select(message => message.GetProperty("content").GetString()));
			return Reply("ok");
		}));
		using var dialog = new OpenAiCompatibleDialog("model", "https://chat.example.test/completions", client);
		dialog.SetSystemPrompt("system");

		var error = await Assert.ThrowsAsync<HttpRequestException>(() =>
			dialog.SendMessage(new ChatRequest { Message = "failed", RememberMessage = rememberMessage }));

		Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
		Assert.Contains("quota exhausted", error.Message);
		Assert.Equal("system", Assert.Single(dialog.GetHistoryCopy()).Content);
		Assert.Equal("ok", await dialog.SendMessage(new ChatRequest { Message = "retry" }));
		Assert.Equal(2, requests);
	}

	[Fact]
	public void SystemPromptRequiresExplicitReplacementAndSurvivesSelectiveClear()
	{
		var store = new InMemoryMessageStore();
		store.AddUserMessage("hello");
		store.AddSystemMessage("original", false);

		Assert.Throws<InvalidOperationException>(() => store.AddSystemMessage("accidental overwrite", false));
		Assert.Equal("original", store.GetHistory()[0].Content);
		store.AddSystemMessage("replacement", true);
		Assert.Equal(new[] { "replacement", "hello" }, store.GetHistory().Select(message => message.Content));

		store.ClearHistory(keepSystemPrompt: true);
		var remaining = Assert.Single(store.GetHistory());
		Assert.Equal(DialogRole.System, remaining.Role);
		Assert.Equal("replacement", remaining.Content);
		store.ClearHistory();
		Assert.Empty(store.GetHistory());
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void InvalidImportedHistoryDoesNotReplaceExistingHistory(bool duplicateSystemPrompt)
	{
		var store = new InMemoryMessageStore();
		store.AddUserMessage("existing");
		IMessage[] invalidHistory = duplicateSystemPrompt
			? [new Message { Role = DialogRole.System }, new Message { Role = DialogRole.Developer }]
			: [new Message { Role = DialogRole.User }, new Message { Role = DialogRole.System }];

		Assert.Throws<InvalidOperationException>(() => store.SetHistory(invalidHistory));

		Assert.Equal("existing", Assert.Single(store.GetHistory()).Content);
	}

	private static HttpResponseMessage Reply(string content) => new(HttpStatusCode.OK)
	{
		Content = JsonContent.Create(new { choices = new[] { new { message = new { role = "assistant", content } } } })
	};
}