using AI.Providers.Configuration.Interfaces;
using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs.Implementations;
using AI.Providers.Dialogs.Interfaces;
using AI.Providers.Factories.Interfaces;

namespace AI.Providers.Factories.Implementations
{
	public class DialogFactory : IDialogFactory
	{
		private readonly IConfigurationProvider configurationProvider;
		public DialogFactory(IConfigurationProvider configurationProvider)
		{
			this.configurationProvider = configurationProvider;
		}

		public IChatDialog Create(ProviderType providerType)
			=> Create(providerType, null);

		public IChatDialog Create(ProviderType providerType, HttpClient? client)
		{
			IChatDialog dialog = providerType switch
			{
				ProviderType.ChatGPT => new ChatGPTDialog(configurationProvider, client),
				ProviderType.GigaChat => new GigaChatDialog(configurationProvider, client),
				ProviderType.DeepSeek => new DeepSeekDialog(configurationProvider, client),
				ProviderType.YandexGPT => new YandexGPTDialog(configurationProvider, client),
				ProviderType.DefaultDialog => new OpenAiCompatibleDialog(configurationProvider, client),
				_ => throw new NotImplementedException()
			};

			return dialog;
		}
	}
}