using AI.Providers.Data.Enumerations;

namespace AI.Providers.Data.Vectorizers
{
	public sealed class Embedder
	{
		public string Model { get; }
		public int Lenght { get; }
		public ProviderType ProviderType {  get; }

		public Embedder(string model, int lenght, ProviderType providerType)
		{
			Model = model;
			Lenght = lenght;
			ProviderType = providerType;
		}
	}
}
