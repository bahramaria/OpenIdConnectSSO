using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OpenIdConnectSSO.AuthServer.Data;

using System.Net;
using System.Text.RegularExpressions;

using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class AuthServerIntegrationTests : IClassFixture<AuthServerFactory>
{
    private readonly AuthServerFactory _factory;

    public AuthServerIntegrationTests(AuthServerFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginPage_ReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authorize_WithoutLogin_RedirectsToLogin()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(
            "/connect/authorize?client_id=sampleclient&response_type=code&redirect_uri=https%3A%2F%2Flocalhost%3A7002%2Fsignin-oidc&scope=openid%20profile%20email%20roles&code_challenge=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa&code_challenge_method=S256");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Login_ThenAuthorize_ReturnsAuthorizationCodeRedirect()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var token = ExtractAntiForgeryToken(loginPage);

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "Admin",
            ["Password"] = "123456",
            ["IsPersistent"] = "false",
            ["__RequestVerificationToken"] = token
        });

        var loginResponse = await client.PostAsync("/Account/Login", form);

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        var authorizeResponse = await client.GetAsync(
            "/connect/authorize?client_id=sampleclient&response_type=code&redirect_uri=https%3A%2F%2Flocalhost%3A7002%2Fsignin-oidc&scope=openid%20profile%20email%20roles&code_challenge=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa&code_challenge_method=S256");

        Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);
        Assert.NotNull(authorizeResponse.Headers.Location);
        Assert.Equal("https", authorizeResponse.Headers.Location!.Scheme);
        Assert.Equal("localhost", authorizeResponse.Headers.Location.Host);
        Assert.Contains("code=", authorizeResponse.Headers.Location.Query);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\\\"__RequestVerificationToken\\\"[^>]*value=\\\"([^\\\"]+)\\\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The login page did not contain an anti-forgery token.");
        return match.Groups[1].Value;
    }
}

public sealed class AuthServerFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("AuthServerIntegrationTests");
                options.UseOpenIddict();
            });
        });
    }
}
