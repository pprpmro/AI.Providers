using AI.Providers.Data.Dialogs;
using AI.Providers.Dialogs;

namespace AI.Providers.Dialogs.Interfaces
{
	/// <summary>Represents a conversation with an AI provider.</summary>
	public interface IChatDialog : IDisposable
	{
		/// <summary>Sends a request using the current conversation history.</summary>
		/// <param name="request">The message, model override, streaming, and history settings.</param>
		/// <param name="onStreamedData">An optional callback for text chunks when <see cref="ChatRequest.Stream"/> is enabled.</param>
		/// <param name="cancellationToken">A token used to cancel the operation.</param>
		/// <returns>The complete assistant reply, including when streaming is enabled.</returns>
		/// <remarks>When <see cref="ChatRequest.RememberMessage"/> is enabled, the new turn is saved after the request succeeds.</remarks>
		Task<string> SendMessage(
			ChatRequest request,
			Action<string>? onStreamedData = null,
			CancellationToken cancellationToken = default
		);
		/// <summary>Clears the conversation history.</summary>
		/// <param name="keepSystemPrompt">Whether to keep the current system prompt.</param>
		void ClearHistory(bool keepSystemPrompt = false);
		/// <summary>Sets the system prompt for subsequent requests.</summary>
		/// <param name="message">The prompt text.</param>
		/// <param name="overrideIfExists">Whether to replace an existing system prompt.</param>
		/// <exception cref="InvalidOperationException">A system prompt already exists and replacement was not requested.</exception>
		void SetSystemPrompt(string message, bool overrideIfExists = false);
		/// <summary>Replaces the conversation history.</summary>
		/// <param name="customMessagesHistory">Messages in conversation order, with at most one system prompt at the beginning.</param>
		void SetHistory(IEnumerable<IMessage> customMessagesHistory);
		/// <summary>Returns a copy of the conversation history in order.</summary>
		List<IMessage> GetHistoryCopy();
	}
}
