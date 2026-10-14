using AI.Providers.Data.Dialogs;
using AI.Providers.Dialogs;

namespace AI.Providers.Dialogs.Interfaces
{
	public interface IChatDialog : IDisposable
	{
		Task<string> SendMessage(
			ChatRequest request,
			Action<string>? onStreamedData = null,
			CancellationToken cancellationToken = default
		);
		void ClearHistory(bool keepSystemPrompt = false);
		void SetSystemPrompt(string message, bool overrideIfExists = false);
		void SetHistory(IEnumerable<IMessage> customMessagesHistory);
		List<IMessage> GetHistoryCopy();
	}
}
