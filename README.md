# AI.Providers

AI.Providers is a .NET 10 library for multi-turn conversations with AI models. A shared dialog API works with adapters for OpenAI (ChatGPT), GigaChat, YandexGPT, DeepSeek, and OpenAI-compatible endpoints, including local servers.

- Send messages, keep conversation history, and set a system prompt.
- Stream replies as they arrive or await a complete response.
- Discover chat models and select a model for each request.
- Create embeddings for search and similarity with OpenAI, GigaChat, and YandexGPT.

## Install

```sh
dotnet add package AI.Providers --version 0.1.0
```

## Stream a reply with application-owned history

The application supplies its settings and loads the conversation history. Keep one dialog for the conversation and pass the history in before each request, as a desktop chat or API endpoint would.

This .NET 10 console example reads support questions from the user. Set `OPENAI_API_KEY` before running it.

```csharp
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Data.Dialogs;
using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs;
using AI.Providers.Factories.Implementations;

var settings = new Dictionary<string, string>
{
    [ConfigurationKeys.ChatGPT.ApiKey] = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new InvalidOperationException("Set OPENAI_API_KEY first.")
};
var config = new DictionaryAccessorConfigurationProvider(() => settings);
var provider = ProviderType.ChatGPT;
var selectedModel = Environment.GetEnvironmentVariable("CHAT_MODEL");

using var httpClient = new HttpClient();
using var dialog = new DialogFactory(config).Create(provider, httpClient);
var history = new List<IMessage>
{
    new Message { Role = DialogRole.System, Content = "Help the user resolve account access issues." }
};

Console.WriteLine("Enter a support question. Submit a blank line to exit.");
while (Console.ReadLine() is { } userMessage && !string.IsNullOrWhiteSpace(userMessage))
{
    dialog.SetHistory(history);
    var reply = await dialog.SendMessage(new ChatRequest
    {
        Message = userMessage,
        ModelName = selectedModel,
        Stream = true,
        RememberMessage = false
    }, onStreamedData: Console.Write);

    Console.WriteLine();
    history.Add(new Message { Role = DialogRole.User, Content = userMessage });
    history.Add(new Message { Role = DialogRole.Assistant, Content = reply });
}
```

In a desktop app, the callback can append chunks to the UI. Load and save the history through your application's storage, and serialize requests for each conversation. For a dialog that owns its history, omit `SetHistory` for each turn and leave `RememberMessage` at its default of `true`. An injected `HttpClient` remains owned by the application.

## Provider settings

Supply the settings for the provider you select through your configuration provider. The library does not load secrets from files or environment variables.

| Adapter | Provider type | Settings to supply |
| --- | --- | --- |
| OpenAI | `ChatGPT` | `ConfigurationKeys.ChatGPT.ApiKey` |
| GigaChat | `GigaChat` | `ConfigurationKeys.Sber.AuthorizeData` |
| YandexGPT | `YandexGPT` | `ConfigurationKeys.YandexGPT.ApiKey` and `ConfigurationKeys.YandexGPT.CatalogId` |
| DeepSeek | `DeepSeek` | `ConfigurationKeys.DeepSeek.ApiKey` |
| OpenAI-compatible | `DefaultDialog` | `ConfigurationKeys.DefaultGPT.DialogEndpoint` and `ConfigurationKeys.DefaultGPT.DialogModel`; supply `ModelsEndpoint` for model discovery and `ApiKey` if required |

For the OpenAI-compatible adapter, configure the full chat completions and model listing URLs. Endpoints and model defaults are defined in `Defaults` and can be overridden through `ConfigurationKeys`.

## Populate a model picker

Reuse the configuration and provider from the chat example. Use the returned model names in your picker and pass the selected ID as `ChatRequest.ModelName`.

```csharp
using AI.Providers.ModelCatalog.Implementations;

var catalogService = new ModelCatalogService(new ModelCatalogProviderFactory(config));
var catalog = await catalogService.GetModelsAsync(provider);

if (!catalog.IsAvailable)
{
    Console.Error.WriteLine(catalog.ErrorMessage);
}
else
{
    foreach (var model in catalog.Models)
        Console.WriteLine($"{model.Name} ({model.Id})");
}
```

## Embed documents for semantic search

Reuse the same configuration and HTTP client. Batch your document text, then pair each returned vector with its document ID before sending the points to your vector store.

```csharp
using AI.Providers.Configuration.Providers;
using AI.Providers.Data.Vectorizers;

var documents = new[]
{
    (Id: "reset-password", Text: "Use Forgot password on the sign-in screen to request a reset link."),
    (Id: "change-email", Text: "Open Account settings to update the email address on your profile.")
};

var embeddingModel = ChatGPTConfiguration.EmbeddingModel(config);
using var vectorizer = new VectorizerFactory(config).Create(embeddingModel, httpClient);
var result = await vectorizer.GetEmbeddingAsync(new VectorizerRequest
{
    Input = documents.Select(document => document.Text).ToArray()
});

var points = documents.Select((document, index) => new
{
    document.Id,
    Vector = result.Embedding[index]
}).ToArray();
```

For GigaChat, use `SberConfiguration.EmbeddingModel(config)` with the GigaChat credentials. For YandexGPT, use `YandexGPTConfiguration.EmbedderBigTexts(config)` for documents and `EmbedderSmallTexts(config)` for short search queries. The YandexGPT adapter sends each input separately and returns the vectors in input order.

The factory picks the model from the embedding configuration. To override it for a single request, set `VectorizerRequest.Model`. All adapters return `float[][]`. OpenAI can also use `Encoding_format = "base64"` on the wire; the adapter decodes it into floats.

## SaluteSpeech

The Sber OAuth helper can also be used by SaluteSpeech integrations; audio capture and speech recognition are handled by the calling application.

## License

MIT. See [LICENSE](LICENSE).
