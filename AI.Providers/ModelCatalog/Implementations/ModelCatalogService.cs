using AI.Providers.Data.Enumerations;
using AI.Providers.Factories.Interfaces;
using AI.Providers.ModelCatalog.Interfaces;

namespace AI.Providers.ModelCatalog.Implementations
{
	public class ModelCatalogService : IModelCatalogService
	{
		private static readonly string[] ChatGptDialogPrefixes = { "gpt-", "chatgpt-", "o1", "o3", "o4" };
		private static readonly string[] NonDialogFragments = { "audio", "dall-e", "embedding", "image", "moderation", "realtime", "transcribe", "tts", "whisper" };

		private readonly IModelCatalogProviderFactory providerFactory;

		public ModelCatalogService(IModelCatalogProviderFactory providerFactory)
		{
			this.providerFactory = providerFactory;
		}

		public async Task<ProviderModelCatalog> GetModelsAsync(
			ProviderType provider,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			ProviderModelCatalog catalog;
			try
			{
				using var modelCatalogProvider = providerFactory.Create(provider);
				var models = await modelCatalogProvider.GetModelsAsync(cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				catalog = new ProviderModelCatalog(provider, FilterDialogModels(provider, models), true);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				catalog = new ProviderModelCatalog(provider, [], false, ex.Message);
			}

			return catalog;
		}

		private static IReadOnlyList<ProviderModel> FilterDialogModels(ProviderType provider, IReadOnlyList<ProviderModel> models)
		{
			return provider switch
			{
				ProviderType.ChatGPT => models.Where(IsChatGptDialogModel).ToList(),
				ProviderType.GigaChat => models.Where(IsGigaChatDialogModel).ToList(),
				ProviderType.YandexGPT => models.Where(IsYandexDialogModel).ToList(),
				ProviderType.DeepSeek => models.Where(IsDeepSeekDialogModel).ToList(),
				_ => models
			};
		}

		private static bool IsChatGptDialogModel(ProviderModel model)
		{
			var id = model.Id;
			if (ContainsAny(id, NonDialogFragments))
				return false;

			return StartsWithAny(id, ChatGptDialogPrefixes);
		}

		private static bool IsGigaChatDialogModel(ProviderModel model)
		{
			return model.Id.Contains("gigachat", StringComparison.OrdinalIgnoreCase)
				&& !ContainsAny(model.Id, NonDialogFragments);
		}

		private static bool IsYandexDialogModel(ProviderModel model)
		{
			return model.Id.StartsWith("gpt://", StringComparison.OrdinalIgnoreCase)
				&& !ContainsAny(model.Id, NonDialogFragments);
		}

		private static bool IsDeepSeekDialogModel(ProviderModel model)
		{
			return model.Id.StartsWith("deepseek-", StringComparison.OrdinalIgnoreCase)
				&& !ContainsAny(model.Id, NonDialogFragments);
		}

		private static bool ContainsAny(string value, IEnumerable<string> fragments)
		{
			return fragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
		}

		private static bool StartsWithAny(string value, IEnumerable<string> prefixes)
		{
			return prefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}
	}
}
