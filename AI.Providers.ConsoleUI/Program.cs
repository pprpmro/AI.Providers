using System.Text;
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs;
using AI.Providers.Factories.Implementations;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("AI.Providers - debug console");

var providers = Enum.GetValues<ProviderType>();
var defaultProvider = (Array.IndexOf(providers, ProviderType.ChatGPT) + 1).ToString();
for (var i = 0; i < providers.Length; i++)
	Console.WriteLine($"{i + 1}. {providers[i]}");

if (!int.TryParse(Read("Provider", defaultProvider), out var selected)
	|| selected < 1 || selected > providers.Length)
{
	Console.WriteLine("Invalid provider number.");
	return;
}

var provider = providers[selected - 1];
var settings = new Dictionary<string, string>();
var keyName = provider switch
{
	ProviderType.ChatGPT => ConfigurationKeys.ChatGPT.ApiKey,
	ProviderType.GigaChat => ConfigurationKeys.Sber.AuthorizeData,
	ProviderType.YandexGPT => ConfigurationKeys.YandexGPT.ApiKey,
	ProviderType.DeepSeek => ConfigurationKeys.DeepSeek.ApiKey,
	_ => ConfigurationKeys.DefaultGPT.ApiKey
};

settings[keyName] = Read(provider == ProviderType.GigaChat
	? "Sber authorization key (Base64)"
	: "API key (optional for local APIs)");

if (provider == ProviderType.YandexGPT)
	settings[ConfigurationKeys.YandexGPT.CatalogId] = Read("Yandex Cloud folder ID");

if (provider == ProviderType.GigaChat)
	settings[ConfigurationKeys.Sber.GigaChatScope] = Read("Scope", Defaults.Sber.GigaChatScope);

if (provider == ProviderType.DefaultDialog)
	settings[ConfigurationKeys.DefaultGPT.DialogEndpoint] = Read(
		"Chat completions URL", Defaults.DefaultGPT.DialogEndpoint);

var configuration = new AccessorConfigurationProvider(key => settings.GetValueOrDefault(key));
var defaultModel = provider switch
{
	ProviderType.ChatGPT => ChatGPTConfiguration.DialogModel(configuration),
	ProviderType.GigaChat => SberConfiguration.DialogModel(configuration),
	ProviderType.YandexGPT => YandexGPTConfiguration.DialogModelLite(configuration),
	ProviderType.DeepSeek => DeepSeekConfiguration.DialogModel(configuration),
	_ => DefaultGPTConfiguration.DialogModel(configuration)
};
var model = Read("Model", defaultModel);

using var dialog = new DialogFactory(configuration).Create(provider);
Console.WriteLine("Type a message. /clear - clear history, /exit - exit.");

while (true)
{
	Console.Write("You: ");
	var message = Console.ReadLine();
	if (message is null || message.Trim() == "/exit")
		break;
	if (string.IsNullOrWhiteSpace(message))
		continue;
	if (message.Trim() == "/clear")
	{
		dialog.ClearHistory();
		Console.WriteLine("History cleared.");
		continue;
	}

	try
	{
		Console.Write("AI: ");
		await dialog.SendMessage(
			new ChatRequest { Message = message, ModelName = model, Stream = true },
			chunk => Console.Write(chunk));
		Console.WriteLine();
	}
	catch (Exception exception)
	{
		Console.WriteLine();
		Console.WriteLine($"Error: {exception.Message}");
	}
}

static string Read(string label, string defaultValue = "")
{
	Console.Write(defaultValue.Length == 0 ? $"{label}: " : $"{label} [{defaultValue}]: ");
	var value = Console.ReadLine()?.Trim();
	return string.IsNullOrEmpty(value) ? defaultValue : value;
}
