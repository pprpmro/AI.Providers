namespace AI.Providers.Configuration.Interfaces;

public interface IConfigurationProvider
{
	string? Get(string key);
}