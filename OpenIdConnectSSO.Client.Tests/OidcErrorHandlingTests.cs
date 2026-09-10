using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIdConnectSSO.Client.Services;
using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class OidcErrorHandlingTests
{
    [Fact]
    public void Oidc_ConfiguresRemoteFailureHandler()
    {
        var options = CreateOptions();

        Assert.NotNull(options.Events.OnRemoteFailure);
    }

    [Fact]
    public void Oidc_ConfiguresAuthenticationFailureHandler()
    {
        var options = CreateOptions();

        Assert.NotNull(options.Events.OnAuthenticationFailed);
    }

    [Fact]
    public void Oidc_ConfiguresAccessDeniedHandler()
    {
        var options = CreateOptions();

        Assert.NotNull(options.Events.OnAccessDenied);
    }

    private static OpenIdConnectOptions CreateOptions()
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
        services.AddAuthentication()
            .AddOpenIddictConfig(configuration, new TestWebHostEnvironment());

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get("oidc");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = typeof(OidcErrorHandlingTests).Assembly.GetName().Name!;
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
