using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;

using System.Net;

using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public sealed class AuthorizationRequestValidationTests(AuthServerFactory factory) : IClassFixture<AuthServerFactory>
{
    [Fact]
    public async Task Authorize_WithUnknownClient_IsRejected()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "unknown-client",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile email roles"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Authorize_WithUnregisteredRedirectUri_IsRejected()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://evil.example/callback",
                ["scope"] = "openid profile email roles"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Authorize_WithUnpermittedScope_IsRejected()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile email roles not_allowed_scope"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Authorize_WithUnpermittedResponseType_IsRejected()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "token",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile email roles"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
}