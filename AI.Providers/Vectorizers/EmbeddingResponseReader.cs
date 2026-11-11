using System.Buffers.Binary;
using System.Text.Json;
using AI.Providers.Data.Vectorizers;

namespace AI.Providers.Vectorizers;

internal static class EmbeddingResponseReader
{
	public static async Task<VectorizerResponse> ReadAsync(
		HttpResponseMessage response,
		int expectedCount,
		CancellationToken cancellationToken)
	{
		response.EnsureSuccessStatusCode();
		using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
		var root = document.RootElement;
		if (root.ValueKind != JsonValueKind.Object ||
			!root.TryGetProperty("data", out var data) ||
			data.ValueKind != JsonValueKind.Array ||
			data.GetArrayLength() != expectedCount)
		{
			throw new JsonException($"Expected {expectedCount} embeddings in the provider response.");
		}

		var embeddings = new float[expectedCount][];
		var position = 0;
		foreach (var item in data.EnumerateArray())
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("embedding", out var embedding))
				throw new JsonException("An embedding is missing from the provider response.");

			var index = position++;
			if (item.TryGetProperty("index", out var itemIndex) &&
				(itemIndex.ValueKind != JsonValueKind.Number || !itemIndex.TryGetInt32(out index)))
			{
				throw new JsonException("The provider returned an invalid embedding index.");
			}

			if (index < 0 || index >= embeddings.Length || embeddings[index] is not null)
				throw new JsonException("The provider returned a duplicate or out-of-range embedding index.");

			embeddings[index] = ReadVector(embedding);
		}

		return new VectorizerResponse { Embedding = embeddings };
	}

	private static float[] ReadVector(JsonElement value)
	{
		if (value.ValueKind == JsonValueKind.Array)
		{
			var vector = new float[value.GetArrayLength()];
			var index = 0;
			foreach (var element in value.EnumerateArray())
			{
				if (element.ValueKind != JsonValueKind.Number || !element.TryGetSingle(out var number) || !float.IsFinite(number))
					throw new JsonException("The embedding contains an invalid number.");
				vector[index++] = number;
			}
			return vector;
		}

		if (value.ValueKind != JsonValueKind.String)
			throw new JsonException("Expected an array of numbers or a base64 embedding.");

		byte[] bytes;
		try
		{
			bytes = Convert.FromBase64String(value.GetString()!);
		}
		catch (FormatException exception)
		{
			throw new JsonException("The embedding is not valid base64.", exception);
		}

		if (bytes.Length == 0 || bytes.Length % sizeof(float) != 0)
			throw new JsonException("The base64 embedding does not contain a float vector.");

		var decoded = new float[bytes.Length / sizeof(float)];
		for (var i = 0; i < decoded.Length; i++)
			decoded[i] = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(i * sizeof(float), sizeof(float)));
		return decoded;
	}
}
