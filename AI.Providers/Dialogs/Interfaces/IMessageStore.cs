using AI.Providers.Data.Dialogs;

namespace AI.Providers.Dialogs.Interfaces
{
	public interface IMessageStore
	{
		void SetHistory(IEnumerable<IMessage> messages);

		void AddMessage(IMessage message);
		void AddUserMessage(string content);
		void AddAssistantMessage(string content);
		void AddSystemMessage(string content, bool overrideIfExists);

		void RemoveMessage(IMessage message);
		void RemoveLastMessages(int count);

		IReadOnlyList<IMessage> GetHistory();
		IReadOnlyList<IMessage> GetLastMessages(int count, bool includeSystemPrompt = true);

		void ClearHistory(bool keepSystemPrompt = false);

		int Count { get; }
		bool HasSystemPrompt { get; }
	}
}
