using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIdConnectSSO.Client.Services;
using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class AuthenticationFlowTests
{
    [Fact]
    public void Oidc_UsesAuthorizationCodeFlow()
    {
        var options = CreateOptions();

        Assert.Equal("code", options.ResponseType);
        Assert.Contains("openid", options.Scope);
        Assert.Contains("profile", options.Scope);
        Assert.Contains("email", options.Scope);
        Assert.Contains("roles", options.Scope);
    }

    [Fact]
    public void Oidc_SavesTokens_AndLoadsUserInfo()
    {
        var options = CreateOptions();

        Assert.True(options.SaveTokens);
        Assert.True(options.GetClaimsFromUserInfoEndpoint);
    }

    [Fact]
    public void Oidc_UsesConfiguredAuthorityAndClient()
    {
        var options = CreateOptions();

        Assert.Equal("https://localhost:7001", options.Authority);
        Assert.Equal("sampleclient", options.ClientId);
        Assert.Equal("very long client secret!!!", options.ClientSecret);
    }

    [Fact]
    public void Oidc_UsesHttpsMetadataOutsideDevelopment()
    {
        var options = CreateOptions(isDevelopment: false);

        Assert.True(options.RequireHttpsMetadata);
    }

    private static OpenIdConnectOptions CreateOptions(bool isDevelopment = true)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Oidc:Authority"] = "https://localhost:7001",
                ["Oidc:ClientId"] = "sampleclient",
                ["Oidc:ClientSecret"] = "very long client secret!!!",
                ["Oidc:ResponseType"] = "code",
                ["Oidc:Scopes"] = "openid profile email roles"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(isDevelopment));
        services.AddAuthentication()
            .AddOpenIddictConfig(configuration, new TestWebHostEnvironment(isDevelopment));

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get("oidc");
    }

    private sealed class TestWebHostEnvironment(bool development) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = development ? "Development" : "Production";
        public string ApplicationName { get; set; } = typeof(AuthenticationFlowTests).Assembly.GetName().Name!;
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
