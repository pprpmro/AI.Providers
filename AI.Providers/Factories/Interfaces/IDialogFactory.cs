using AI.Providers.Data.Enumerations;
using AI.Providers.Dialogs.Interfaces;

namespace AI.Providers.Factories.Interfaces
{
	/// <summary>Creates chat dialogs for supported AI providers.</summary>
	public interface IDialogFactory
	{
		/// <summary>Creates a dialog with its own HTTP client.</summary>
		/// <param name="providerType">The provider to use.</param>
		/// <returns>A new dialog that the caller must dispose.</returns>
		IChatDialog Create(ProviderType providerType);
		/// <summary>Creates a dialog that can reuse a caller-supplied HTTP client.</summary>
		/// <param name="providerType">The provider to use.</param>
		/// <param name="client">An optional HTTP client owned by the caller. Pass <see langword="null"/> to create a client for this instance.</param>
		/// <returns>A new dialog that the caller must dispose.</returns>
		IChatDialog Create(ProviderType providerType, HttpClient? client);
	}
}