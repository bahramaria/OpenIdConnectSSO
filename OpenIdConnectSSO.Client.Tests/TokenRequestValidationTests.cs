using System.Net;

using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public sealed class TokenRequestValidationTests : IClassFixture<AuthServerFactory>
{
    private readonly AuthServerFactory _factory;

    public TokenRequestValidationTests(AuthServerFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Token_WithUnknownClient_IsRejected()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = "unknown-client",
            ["client_secret"] = "invalid-secret",
            ["code"] = "invalid-code",
            ["redirect_uri"] = "https://localhost:7002/signin-oidc",
            ["code_verifier"] = "invalid-code-verifier"
        });

        var response = await client.PostAsync("/connect/token", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Token_WithUnsupportedGrantType_IsRejected()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "sampleclient",
            ["client_secret"] = "very long client secret!!!"
        });

        var response = await client.PostAsync("/connect/token", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
