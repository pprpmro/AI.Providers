using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.Providers.Common
{
	public class SberAccessTokenProvider
	{
		public SberAccessTokenProvider(HttpClient client, string accessTokenEndpoint, string scope, string credentials)
		{
			this.accessTokenEndpoint = accessTokenEndpoint ?? throw new ArgumentNullException(nameof(accessTokenEndpoint));
			this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
			this.credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
			httpClient = client;
			RqUID = Guid.NewGuid();
		}

		protected readonly HttpClient httpClient;
		protected readonly string accessTokenEndpoint;
		protected readonly string scope;
		protected readonly string credentials;
		private readonly Guid RqUID;
		private SberAccessData? accessData;

		protected string Scope => scope;

		public async Task EnsureAccessDataAsync(CancellationToken cancellationToken = default)
		{
			if (accessData == null || accessData.isExpired)
			{
				var newAccessData = await GetAccessDataAsync(cancellationToken);
				accessData = newAccessData;
			}
		}

		public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
		{
			await EnsureAccessDataAsync(cancellationToken);
			return accessData!.AccessToken;
		}

		private async Task<SberAccessData> GetAccessDataAsync(CancellationToken cancellationToken)
		{
			var scopeList = new List<KeyValuePair<string, string>>()
			{
				new("scope", Scope)
			};

			using var request = new HttpRequestMessage(HttpMethod.Post, accessTokenEndpoint)
			{
				Content = new FormUrlEncodedContent(scopeList)
			};
			request.Headers.Add("RqUID", RqUID.ToString());
			request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

			using var response = await httpClient.SendAsync(request, cancellationToken);
			var accessData = await ReadAccessDataAsync(response, cancellationToken);

			if (accessData == null)
			{
				throw new NullReferenceException(nameof(accessData));
			}

			return accessData;
		}

		private static async Task<SberAccessData?> ReadAccessDataAsync(HttpResponseMessage? response, CancellationToken cancellationToken)
		{
			if (response == null)
				throw new NullReferenceException(nameof(response));

			string result = await response.Content.ReadAsStringAsync(cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode}. Body: {result}", null, response.StatusCode);
			}

			return JsonSerializer.Deserialize<SberAccessData>(result);
		}

		protected class SberAccessData
		{
			[JsonPropertyName("access_token")]
			public string AccessToken { get; set; } = string.Empty;
			[JsonPropertyName("expires_at")]
			public long? ExpiresAt { get; set; }
			[JsonPropertyName("code")]
			public int Code { get; set; }
			[JsonPropertyName("message")]
			public string Message { get; set; } = string.Empty;

			public bool isExpired
			{
				get
				{
					if (!ExpiresAt.HasValue)
					{
						return true;
					}
					else
					{
						return ExpiresAt.Value <= DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeMilliseconds();
					}
				}
			}
		}
	}
}
