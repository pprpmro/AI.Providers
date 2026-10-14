using AI.Providers.Configuration.Interfaces;

namespace AI.Providers.Configuration.Implementations;

public sealed class DictionaryAccessorConfigurationProvider : IConfigurationProvider
{
	private readonly Func<IReadOnlyDictionary<string, string>> dictionaryAccessor;

	public DictionaryAccessorConfigurationProvider(Func<IReadOnlyDictionary<string, string>> dictionaryAccessor)
	{
		this.dictionaryAccessor = dictionaryAccessor ?? throw new ArgumentNullException(nameof(dictionaryAccessor));
	}

	public string? Get(string key)
	{
		ArgumentNullException.ThrowIfNull(key);

		var dictionary = dictionaryAccessor();

		return dictionary.TryGetValue(key, out var value)
			? value
			: null;
	}
}