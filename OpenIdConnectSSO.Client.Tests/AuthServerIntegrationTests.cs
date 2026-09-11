using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OpenIdConnectSSO.AuthServer.Data;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class AuthServerIntegrationTests
{
    [Fact]
    public async Task LoginPage_ReturnsSuccess()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authorize_WithoutLogin_RedirectsToLogin()
    {
        using var factory = new AuthServerFactory();
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
                ["scope"] = "openid profile email roles",
                ["code_challenge"] = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                ["code_challenge_method"] = "S256"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Login_ThenAuthorize_ReturnsAuthorizationCodeRedirect()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
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

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile email roles",
                ["code_challenge"] = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                ["code_challenge_method"] = "S256"
            });

        var authorizeResponse = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);
        Assert.NotNull(authorizeResponse.Headers.Location);
        Assert.Equal("https", authorizeResponse.Headers.Location!.Scheme);
        Assert.Equal("localhost", authorizeResponse.Headers.Location.Host);
        Assert.Contains("code=", authorizeResponse.Headers.Location.Query);
    }

    [Fact]
    public async Task Authorize_WithInvalidRedirectUri_IsRejected()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var antiForgeryToken = ExtractAntiForgeryToken(loginPage);

        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "Admin",
            ["Password"] = "123456",
            ["IsPersistent"] = "false",
            ["__RequestVerificationToken"] = antiForgeryToken
        });

        var loginResponse = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://evil.example/callback",
                ["scope"] = "openid profile email roles",
                ["code_challenge"] = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                ["code_challenge_method"] = "S256"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Authorize_WithoutPkce_IsRejected()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var antiForgeryToken = ExtractAntiForgeryToken(loginPage);

        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "Admin",
            ["Password"] = "123456",
            ["IsPersistent"] = "false",
            ["__RequestVerificationToken"] = antiForgeryToken
        });

        var loginResponse = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile",
                ["code_challenge"] = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                ["code_challenge_method"] = "S256"
            });

        var response = await client.GetAsync(authorizeUrl);

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Token_WithUnknownClient_IsRejected()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        using var tokenForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = "unknown-client",
            ["client_secret"] = "invalid-secret",
            ["code"] = "invalid-code",
            ["redirect_uri"] = "https://localhost:7002/signin-oidc",
            ["code_verifier"] = "invalid-verifier"
        });

        var response = await client.PostAsync("/connect/token", tokenForm);
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using var json = JsonDocument.Parse(payload);
        Assert.Equal("invalid_client", json.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Login_ThenAuthorize_ThenToken_ThenUserInfo_Succeeds()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var antiForgeryToken = ExtractAntiForgeryToken(loginPage);

        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "Admin",
            ["Password"] = "123456",
            ["IsPersistent"] = "false",
            ["__RequestVerificationToken"] = antiForgeryToken
        });

        var loginResponse = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        const string codeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        const string codeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile email roles",
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256"
            });

        var authorizeResponse = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);
        Assert.NotNull(authorizeResponse.Headers.Location);

        var code = GetQueryParameter(authorizeResponse.Headers.Location!, "code");
        Assert.False(string.IsNullOrWhiteSpace(code));

        using var tokenForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = "sampleclient",
            ["client_secret"] = "very long client secret!!!",
            ["code"] = code!,
            ["redirect_uri"] = "https://localhost:7002/signin-oidc",
            ["code_verifier"] = codeVerifier
        });

        var tokenResponse = await client.PostAsync("/connect/token", tokenForm);
        var tokenPayload = await tokenResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        using var tokenJson = JsonDocument.Parse(tokenPayload);
        var tokenRoot = tokenJson.RootElement;

        var accessToken = tokenRoot.GetProperty("access_token").GetString();
        var idToken = tokenRoot.GetProperty("id_token").GetString();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(idToken));
        Assert.Equal("Bearer", tokenRoot.GetProperty("token_type").GetString());

        using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userInfoResponse = await client.SendAsync(userInfoRequest);
        var userInfoPayload = await userInfoResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);

        using var userInfoJson = JsonDocument.Parse(userInfoPayload);
        var userInfoRoot = userInfoJson.RootElement;

        Assert.False(string.IsNullOrWhiteSpace(userInfoRoot.GetProperty("sub").GetString()));
        Assert.Equal("Admin", userInfoRoot.GetProperty("name").GetString());
        Assert.Contains(
            "Admin",
            userInfoRoot.GetProperty("roles").EnumerateArray().Select(element => element.GetString()));

        using var replayTokenForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = "sampleclient",
            ["client_secret"] = "very long client secret!!!",
            ["code"] = code!,
            ["redirect_uri"] = "https://localhost:7002/signin-oidc",
            ["code_verifier"] = codeVerifier
        });

        var replayResponse = await client.PostAsync("/connect/token", replayTokenForm);

        Assert.Equal(HttpStatusCode.BadRequest, replayResponse.StatusCode);
    }

    [Fact]
    public async Task Token_WithInvalidPkceVerifier_IsRejected()
    {
        using var factory = new AuthServerFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var antiForgeryToken = ExtractAntiForgeryToken(loginPage);

        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "Admin",
            ["Password"] = "123456",
            ["IsPersistent"] = "false",
            ["__RequestVerificationToken"] = antiForgeryToken
        });

        var loginResponse = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        const string codeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        const string codeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        string authorizeUrl = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = "sampleclient",
                ["response_type"] = "code",
                ["redirect_uri"] = "https://localhost:7002/signin-oidc",
                ["scope"] = "openid profile email roles",
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256"
            });

        var authorizeResponse = await client.GetAsync(authorizeUrl);

        Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);
        Assert.NotNull(authorizeResponse.Headers.Location);

        var code = GetQueryParameter(authorizeResponse.Headers.Location!, "code");
        Assert.False(string.IsNullOrWhiteSpace(code));

        using var tokenForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = "sampleclient",
            ["client_secret"] = "very long client secret!!!",
            ["code"] = code!,
            ["redirect_uri"] = "https://localhost:7002/signin-oidc",
            ["code_verifier"] = "wrong-code-verifier"
        });

        var tokenResponse = await client.PostAsync("/connect/token", tokenForm);

        Assert.Equal(HttpStatusCode.BadRequest, tokenResponse.StatusCode);
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

    private static string? GetQueryParameter(Uri uri, string name)
    {
        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], name, StringComparison.Ordinal))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}

public sealed class AuthServerFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"AuthServerIntegrationTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Oidc:EncryptionKey:Password", "123456789");
        builder.UseSetting("Seed:Users:AdminPassword", "123456");
        builder.UseSetting("Seed:Users:EmployeePassword", "123456");
        builder.UseSetting("Seed:Clients:SampleClientSecret", "very long client secret!!!");
        builder.UseSetting("Seed:Clients:GatewayClientSecret", "very long client secret!!!");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseOpenIddict();
            });
        });
    }
}