namespace AI.Providers.Dialogs;

public sealed class ChatRequest
{
	public string? Message { get; init; }

	public string? ModelName { get; init; }

	public bool Stream { get; init; }

	public bool RememberMessage { get; init; } = true;
}
