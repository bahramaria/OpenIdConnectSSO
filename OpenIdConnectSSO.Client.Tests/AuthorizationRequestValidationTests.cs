using Microsoft.AspNetCore.Mvc.Testing;

using System.Net;

using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public sealed class AuthorizationRequestValidationTests : IClassFixture<AuthServerFactory>
{
    private readonly AuthServerFactory _factory;

    public AuthorizationRequestValidationTests(AuthServerFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Authorize_WithUnknownClient_IsRejected()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(
            "/connect/authorize?client_id=unknown-client&response_type=code&redirect_uri=https%3A%2F%2Flocalhost%3A7002%2Fsignin-oidc&scope=openid%20profile%20email%20roles");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Authorize_WithUnregisteredRedirectUri_IsRejected()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(
            "/connect/authorize?client_id=sampleclient&response_type=code&redirect_uri=https%3A%2F%2Fevil.example%2Fcallback&scope=openid%20profile%20email%20roles");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Authorize_WithUnpermittedScope_IsRejected()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(
            "/connect/authorize?client_id=sampleclient&response_type=code&redirect_uri=https%3A%2F%2Flocalhost%3A7002%2Fsignin-oidc&scope=openid%20profile%20email%20roles%20not_allowed_scope");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Authorize_WithUnpermittedResponseType_IsRejected()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(
            "/connect/authorize?client_id=sampleclient&response_type=token&redirect_uri=https%3A%2F%2Flocalhost%3A7002%2Fsignin-oidc&scope=openid%20profile%20email%20roles");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
}
