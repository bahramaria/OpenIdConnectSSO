using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System.Security.Cryptography;
using OpenIdConnectSSO.AuthServer.Data;
using OpenIdConnectSSO.Common;

namespace OpenIdConnectSSO.AuthServer.Services;

public static class OpenIddictServerExtensions
{
    public static void AddOpenIddictConfig(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AppDbContext>();
            })
            .AddServer(options =>
            {
                options.AllowAuthorizationCodeFlow()
                       .RequireProofKeyForCodeExchange();

                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");
                options.SetUserinfoEndpointUris("/connect/userinfo");

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Roles);

                var rsaSecurityKey = CreateRsaSecurityKey(config, env);
                options.AddSigningKey(rsaSecurityKey);
                options.AddEncryptionKey(rsaSecurityKey);

                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough()
                       .EnableStatusCodePagesIntegration();

                options.AllowRefreshTokenFlow();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }

    public static RsaSecurityKey CreateRsaSecurityKey(IConfiguration config, IWebHostEnvironment env)
    {
        var encryptionKeyUrl = config["Oidc:EncryptionKey:Url"];
        if (string.IsNullOrWhiteSpace(encryptionKeyUrl))
        {
            throw new InvalidOperationException("OIDC encryption key path is not configured.");
        }

        var encryptionKeyPassword = config["Oidc:EncryptionKey:Password"];
        if (string.IsNullOrWhiteSpace(encryptionKeyPassword))
        {
            throw new InvalidOperationException("OIDC encryption key password is not configured.");
        }

        var keyPath = Path.Combine(env.MapPath(encryptionKeyUrl));
        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException("OIDC encryption key file was not found.", keyPath);
        }

        using var key = RSA.Create();
        key.ImportFromEncryptedPem(File.ReadAllText(keyPath), encryptionKeyPassword);

        if (key.KeySize < 2048)
        {
            throw new InvalidOperationException("The OIDC RSA key must be at least 2048 bits.");
        }

        return new RsaSecurityKey(key.ExportParameters(true));
    }
}
