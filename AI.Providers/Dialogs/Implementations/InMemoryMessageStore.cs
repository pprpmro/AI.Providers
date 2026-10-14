using AI.Providers.Data.Dialogs;
using AI.Providers.Dialogs.Interfaces;

namespace AI.Providers.Dialogs.Implementations
{
	public class InMemoryMessageStore : IMessageStore
	{
		private readonly List<IMessage> messages = [];

		public InMemoryMessageStore()
		{
		}

		public InMemoryMessageStore(IEnumerable<IMessage>? initialMessages = null)
		{
			if (initialMessages != null)
			{
				SetHistory(initialMessages);
			}
		}

		public int Count => messages.Count;
		public bool HasSystemPrompt => messages.Count > 0 && (messages[0].Role == DialogRole.System || messages[0].Role == DialogRole.Developer);

		public void SetHistory(IEnumerable<IMessage> newMessages)
		{
			ArgumentNullException.ThrowIfNull(newMessages);
			List<IMessage> newMessagesList = newMessages as List<IMessage> ?? newMessages.ToList();
			ValidateHistory(newMessagesList);

			ClearHistory();
			messages.AddRange(newMessagesList);
		}

		public void AddMessage(IMessage message) => messages.Add(message);
		public void AddUserMessage(string content) => AddMessage(
			new Message()
			{
				Role = DialogRole.User,
				Content = content
			}
		);
		public void AddAssistantMessage(string content) => AddMessage(
			new Message()
			{
				Role = DialogRole.Assistant,
				Content = content
			}
		);

		public void AddSystemMessage(string content, bool overrideIfExists)
		{
			if (HasSystemPrompt)
			{
				if (!overrideIfExists)
				{
					throw new InvalidOperationException("System prompt already exists. Use overrideIfExists = true to replace it.");
				}
				messages[0].Content = content;
			}
			else
			{
				var message = new Message()
				{
					Role = DialogRole.System,
					Content = content
				};
				messages.Insert(0, message);
			}
		}

		public void RemoveMessage(IMessage message) => messages.Remove(message);
		public void RemoveLastMessages(int count)
		{
			if (count <= 0 || messages.Count == 0)
			{
				return;
			}

			int preserveIndex = HasSystemPrompt ? 1 : 0;
			int removableCount = messages.Count - preserveIndex;
			int actualRemoveCount = Math.Min(count, removableCount);

			if (actualRemoveCount <= 0)
			{
				return;
			}

			int startIndex = messages.Count - actualRemoveCount;

			messages.RemoveRange(startIndex, actualRemoveCount);
		}

		public void ClearHistory(bool keepSystemPrompt = false)
		{
			if (!keepSystemPrompt || !HasSystemPrompt)
			{
				messages.Clear();
				return;
			}

			if (messages.Count > 1)
			{
				messages.RemoveRange(1, messages.Count - 1);
			}
		}

		public IReadOnlyList<IMessage> GetHistory() => messages.AsReadOnly();


		public IReadOnlyList<IMessage> GetLastMessages(int count, bool includeSystemPrompt)
		{
			if (!includeSystemPrompt)
			{
				return messages.TakeLast(count).ToList();
			}

			int totalMessages = messages.Count;

			if (totalMessages == 0)
			{
				return Array.Empty<IMessage>();
			}

			var first = messages[0];
			bool hasSystemPrompt = first.Role == DialogRole.System || first.Role == DialogRole.Developer;

			int tailStart = hasSystemPrompt ? 1 : 0;
			int tailCount = Math.Min(count, totalMessages - tailStart);
			var result = new List<IMessage>(capacity: (hasSystemPrompt ? 1 : 0) + tailCount);

			if (hasSystemPrompt)
			{
				result.Add(first);
			}

			if (tailCount > 0)
			{
				result.AddRange(messages.Skip(totalMessages - tailCount));
			}

			return result;
		}

		private void ValidateHistory(List<IMessage> messagesToValidate)
		{
			if (messagesToValidate.Count == 0)
				return;

			var systemMessages = messagesToValidate
				.Where(m => m.Role == DialogRole.System || m.Role == DialogRole.Developer)
				.ToList();

			if (systemMessages.Count > 1)
				throw new InvalidOperationException("Custom history contains more than one system prompt.");

			if (systemMessages.Count == 1 && !ReferenceEquals(messagesToValidate[0], systemMessages[0]))
				throw new InvalidOperationException("System prompt must be the first message in the history.");
		}
	}
}
