using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs.Interfaces;

namespace AI.Providers.Factories.Interfaces
{
	public interface IDialogFactory
	{
		IChatDialog Create(ProviderType providerType);
		IChatDialog Create(ProviderType providerType, HttpClient? client);
	}
}