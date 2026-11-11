namespace AI.Providers.Dialogs;

/// <summary>Settings for a single request in a chat dialog.</summary>
public sealed class ChatRequest
{
	/// <summary>Gets the user message to send with the existing history, if any.</summary>
	public string? Message { get; init; }

	/// <summary>Gets the model override for this request. If <see langword="null"/>, the dialog uses its configured model.</summary>
	public string? ModelName { get; init; }

	/// <summary>Gets whether the provider should stream the reply in chunks.</summary>
	public bool Stream { get; init; }

	/// <summary>Gets whether a successful turn should be appended to the dialog history.</summary>
	public bool RememberMessage { get; init; } = true;
}
