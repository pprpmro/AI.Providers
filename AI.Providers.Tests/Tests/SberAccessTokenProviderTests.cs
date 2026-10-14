using System.Net;
using System.Net.Http.Json;
using AI.Providers.Common;
using AI.Providers.Configuration;
using AI.Providers.Configuration.Implementations;
using AI.Providers.Configuration.Providers;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class SberAccessTokenProviderTests
{
    [Fact]
    public async Task SaluteUsesApplicationCredentialsAndReusesUnexpiredToken()
    {
        var settings = new Dictionary<string, string>
        {
            [ConfigurationKeys.Sber.AccessTokenEndpoint] = "https://auth.example.test/oauth",
            [ConfigurationKeys.Sber.SaluteSpeechCredentials] = "test-credentials",
            [ConfigurationKeys.Sber.AuthorizeData] = "other-gigachat-credentials",
            [ConfigurationKeys.Sber.SaluteSpeechScope] = "test-salute-scope"
        };
        var configuration = new DictionaryAccessorConfigurationProvider(() => settings);
        var requests = 0;
        using var client = new HttpClient(new MockHttpMessageHandler(async (request, ct) =>
        {
            requests++;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(settings[ConfigurationKeys.Sber.AccessTokenEndpoint], request.RequestUri!.AbsoluteUri);
            Assert.Equal("Basic", request.Headers.Authorization!.Scheme);
            Assert.Equal("test-credentials", request.Headers.Authorization.Parameter);
            Assert.True(Guid.TryParse(request.Headers.GetValues("RqUID").Single(), out _));
            Assert.Equal("scope=test-salute-scope", await request.Content!.ReadAsStringAsync(ct));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    access_token = "test-access-token",
                    expires_at = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeMilliseconds()
                })
            };
        }));
        var provider = new SberAccessTokenProvider(client,
            SberConfiguration.AccessTokenEndpoint(configuration),
            SberConfiguration.SaluteSpeechScope(configuration),
            SberConfiguration.SaluteSpeechCredentials(configuration));

        await provider.EnsureAccessDataAsync();
        Assert.Equal("test-access-token", await provider.GetAccessTokenAsync());
        Assert.Equal("test-access-token", await provider.GetAccessTokenAsync());

        Assert.Equal(1, requests);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    [Theory]
    [InlineData(-60)]
    [InlineData(10)]
    public async Task TokenIsRefreshedBeforeExpiryAndTheReplacementIsReused(int secondsUntilExpiry)
    {
        var requests = 0;
        using var client = new HttpClient(new MockHttpMessageHandler((_, _) =>
        {
            requests++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    access_token = requests == 1 ? "expired-token" : "fresh-token",
                    expires_at = (requests == 1 ? DateTimeOffset.UtcNow.AddSeconds(secondsUntilExpiry) : DateTimeOffset.UtcNow.AddDays(1)).ToUnixTimeMilliseconds()
                })
            });
        }));
        var provider = new SberAccessTokenProvider(client, "https://auth.example.test/oauth", "scope", "credentials");

        await provider.EnsureAccessDataAsync();
        Assert.Equal("fresh-token", await provider.GetAccessTokenAsync());
        Assert.Equal("fresh-token", await provider.GetAccessTokenAsync());
        Assert.Equal(2, requests);
    }
}
