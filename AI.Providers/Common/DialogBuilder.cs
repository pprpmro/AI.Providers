using AI.Providers.Data.Dialogs;
using AI.Providers.Dialogs;

namespace AI.Providers.Common
{
	public class DialogBuilder
	{
		private readonly List<IMessage> messages = new();
		private DialogBuilder() { }
		public static DialogBuilder NewDialog() => new DialogBuilder();

		public DialogBuilder Add(string role, string content)
		{
			messages.Add(new Message { Role = role ?? "", Content = content ?? "" });
			return this;
		}

		public DialogBuilder System(string content) => Add(DialogRole.System, content);
		public DialogBuilder Dev(string content) => Add(DialogRole.Developer, content);
		public DialogBuilder User(string content) => Add(DialogRole.User, content);
		public DialogBuilder Assistant(string content) => Add(DialogRole.Assistant, content);

		public List<IMessage> Build() => messages;

		public static List<IMessage> Of(params (string role, string content)[] items)
		{
			var b = NewDialog();
			foreach (var (r, c) in items) b.Add(r, c);
			return b.Build();
		}
	}
}
