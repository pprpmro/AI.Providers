using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Interfaces
{
	/// <summary>Stores the ordered messages and system prompt for a conversation.</summary>
	public interface IMessageStore
	{
		/// <summary>Replaces the stored history with the supplied messages.</summary>
		void SetHistory(IEnumerable<IMessage> messages);

		/// <summary>Appends a message to the history.</summary>
		void AddMessage(IMessage message);
		/// <summary>Appends a message with the user role.</summary>
		void AddUserMessage(string content);
		/// <summary>Appends a message with the assistant role.</summary>
		void AddAssistantMessage(string content);
		/// <summary>Inserts a system prompt at the beginning of the history.</summary>
		/// <param name="content">The prompt text.</param>
		/// <param name="overrideIfExists">Whether to replace the current system prompt if one is already present.</param>
		void AddSystemMessage(string content, bool overrideIfExists);

		/// <summary>Removes the specified message if it is present.</summary>
		void RemoveMessage(IMessage message);
		/// <summary>Removes up to the specified number of recent messages, preserving the system prompt.</summary>
		void RemoveLastMessages(int count);

		/// <summary>Gets a read-only list of the stored messages in order.</summary>
		IReadOnlyList<IMessage> GetHistory();
		/// <summary>Gets recent messages in conversation order.</summary>
		/// <param name="count">The maximum number of recent messages to select.</param>
		/// <param name="includeSystemPrompt">Whether to include the system prompt before the selected messages when a prompt is present.</param>
		IReadOnlyList<IMessage> GetLastMessages(int count, bool includeSystemPrompt = true);

		/// <summary>Clears stored messages, optionally preserving the system prompt.</summary>
		void ClearHistory(bool keepSystemPrompt = false);

		/// <summary>Gets the number of stored messages.</summary>
		int Count { get; }
		/// <summary>Gets whether the history begins with a system or developer prompt.</summary>
		bool HasSystemPrompt { get; }
	}
}
