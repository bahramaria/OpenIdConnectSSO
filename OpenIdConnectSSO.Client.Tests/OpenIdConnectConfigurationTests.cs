using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIdConnectSSO.Client.Services;

using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class OpenIdConnectConfigurationTests
{
    [Fact]
    public void AddOpenIddictConfig_UsesBaselineOidcSettings()
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
        services.AddAuthentication().AddOpenIddictConfig(
            configuration,
            new TestWebHostEnvironment());

        using var provider = services.BuildServiceProvider();
        var options = provider
            .GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<OpenIdConnectOptions>>()
            .Get("oidc");

        Assert.Equal("https://localhost:7001", options.Authority);
        Assert.Equal("sampleclient", options.ClientId);
        Assert.Equal("very long client secret!!!", options.ClientSecret);
        Assert.Equal("code", options.ResponseType);
        Assert.True(options.SaveTokens);
        Assert.True(options.GetClaimsFromUserInfoEndpoint);
        Assert.Equal(
            ["openid", "profile", "email", "roles"],
            [.. options.Scope]);
    }

    private sealed class TestWebHostEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "OpenIdConnectSSO.Client.Tests";
        public string EnvironmentName { get; set; } = Microsoft.Extensions.Hosting.Environments.Production;
        public string WebRootPath { get; set; } = string.Empty;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
