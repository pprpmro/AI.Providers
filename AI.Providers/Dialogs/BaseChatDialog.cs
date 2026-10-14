using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AI.Providers.Data.Dialogs;
using AI.Providers.Dialogs.Implementations;
using AI.Providers.Dialogs.Interfaces;

namespace AI.Providers.Dialogs
{
	public abstract class BaseChatDialog<TMessage, TRequest> : IChatDialog, IDisposable
		where TMessage : IMessage, new()
		where TRequest : IRequest, new()
	{
		private readonly IMessageStore messageStore;

		protected HttpClient httpClient;
		protected AuthenticationHeaderValue? authorization;
		private readonly bool ownsHttpClient;

		protected readonly string modelName;
		protected readonly string? completionsEndpoint;

		protected BaseChatDialog(string? modelName, string? completionsEndpoint)
			: this(modelName, completionsEndpoint, null) { }

		protected BaseChatDialog(string? modelName, string? completionsEndpoint, HttpClient? client)
		{
			this.modelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
			this.completionsEndpoint = completionsEndpoint ?? throw new ArgumentNullException(nameof(completionsEndpoint));
			this.messageStore = new InMemoryMessageStore();
			httpClient = client ?? new HttpClient();
			ownsHttpClient = client is null;
		}

		public void Dispose()
		{
			if (ownsHttpClient)
				httpClient.Dispose();
		}

		public int MessageCount { get { return messageStore.Count; } }

		public void ClearHistory(bool keepSystemPrompt = false)
		{
			messageStore.ClearHistory(keepSystemPrompt);
		}

		public virtual void SetSystemPrompt(string message, bool overrideIfExists = false)
		{
			if (string.IsNullOrEmpty(message))
				throw new ArgumentException("System prompt cannot be null or empty.");

			messageStore.AddSystemMessage(message, overrideIfExists);
		}

		public virtual async Task<string> SendMessage(
			ChatRequest chatRequest,
			Action<string>? onStreamedData = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(chatRequest);
			cancellationToken.ThrowIfCancellationRequested();

			var message = chatRequest.Message;

			var requestData = new TRequest()
			{
				Model = chatRequest.ModelName ?? modelName,
				Messages = PreparePrompt(message),
				Stream = chatRequest.Stream,
			};

			using var request = new HttpRequestMessage(HttpMethod.Post, completionsEndpoint)
			{
				Content = JsonContent.Create(requestData)
			};
			request.Headers.Authorization = authorization;
			using var response = await httpClient.SendAsync(
				request,
				chatRequest.Stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead,
				cancellationToken
			);
			if (!response.IsSuccessStatusCode)
			{
				var body = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new HttpRequestException($"Error: {body}", null, response.StatusCode);
			}

			string assistantResponse = chatRequest.Stream
				? await BaseChatDialog<TMessage, TRequest>.ReadStreamedResponse(response, onStreamedData, cancellationToken)
				: await BaseChatDialog<TMessage, TRequest>.ReadDirectResponse(response, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();

			if (chatRequest.RememberMessage)
			{
				if (!string.IsNullOrEmpty(message))
				{
					messageStore.AddUserMessage(message);
				}
				messageStore.AddAssistantMessage(assistantResponse);
			}

			return assistantResponse;
		}

		public void SetHistory(IEnumerable<IMessage> customMessagesHistory)
		{
			ArgumentNullException.ThrowIfNull(customMessagesHistory);
			messageStore.SetHistory(customMessagesHistory.Select(CopyMessage));
		}

		public List<IMessage> GetHistoryCopy()
		{
			return messageStore.GetHistory().Select(CopyMessage).ToList();
		}

		private static async Task<string> ReadStreamedResponse(HttpResponseMessage response, Action<string>? onStreamedData, CancellationToken cancellationToken)
		{
			var resultBuilder = new StringBuilder();
			var completed = false;
			onStreamedData ??= _ => {};

			using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using var reader = new StreamReader(stream);

			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var line = await reader.ReadLineAsync(cancellationToken);
				if (line is null)
				{
					break;
				}
				if (!line.StartsWith("data:", StringComparison.Ordinal))
				{
					continue;
				}

				line = line[5..].Trim();
				if (line == "[DONE]")
				{
					completed = true;
					break;
				}
				if (line.Length == 0)
				{
					continue;
				}

				try
				{
					using var json = JsonDocument.Parse(line);
					var choice = json.RootElement.GetProperty("choices")[0];
					var responseDelta = choice.GetProperty("delta");

					if (choice.TryGetProperty("finish_reason", out var finishReason)
						&& finishReason.ValueKind == JsonValueKind.String
						&& !string.IsNullOrEmpty(finishReason.GetString()))
					{
						completed = true;
					}

					if (responseDelta.TryGetProperty("content", out var content))
					{
						
						var contentText = content.GetString() ?? string.Empty;
						resultBuilder.Append(contentText);
						onStreamedData(contentText);
					}
				}
				catch (Exception ex) when (ex is not OperationCanceledException)
				{
					throw new Exception($"Stream exception: {ex.Message}");
				}
			}

			cancellationToken.ThrowIfCancellationRequested();
			if (!completed)
			{
				throw new HttpRequestException("Streaming response ended before generation completed.");
			}

			return resultBuilder.ToString();
		}
		private static async Task<string> ReadDirectResponse(HttpResponseMessage response, CancellationToken cancellationToken)
		{
			var responseData = await response.Content.ReadFromJsonAsync<ResponseData>(cancellationToken: cancellationToken);
			if (responseData?.Choices == null || responseData.Choices.Count == 0)
			{
				throw new Exception("No choices were returned by the API");
			}

			return responseData.Choices[0].Message.Content?.Trim() ?? string.Empty;
		}

		private static IMessage CopyMessage(IMessage message) => new TMessage
		{
			Role = message.Role,
			Content = message.Content
		};

		private IReadOnlyList<IMessage> PreparePrompt(string? message)
		{
			var messages = messageStore.GetHistory().ToList();
			if (!string.IsNullOrEmpty(message))
			{
				messages.Add(new Message { Role = DialogRole.User, Content = message });
			}
			return messages;
		}
	}
}
