using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;

using System.Security.Cryptography;

using OpenIdConnectSSO.Common;
using OpenIdConnectSSO.AuthServer.Data;

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
                // فعال کردن Authorization Code Flow
                options.AllowAuthorizationCodeFlow()
                       .RequireProofKeyForCodeExchange();

                // مسیرهای استاندارد
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");
                options.SetUserinfoEndpointUris("/connect/userinfo");
                //options.SetLogoutEndpointUris("/connect/logout");

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Roles);

                // کلیدهای امضای Token  
                // برای محیط dev
                //options.AddDevelopmentEncryptionCertificate()
                //       .AddDevelopmentSigningCertificate();

                var rsaSecurityKey = CreateRsaSeruciryKey(config, env);
                options.AddSigningKey(rsaSecurityKey);
                options.AddEncryptionKey(rsaSecurityKey);

                // ادغام با ASP.NET Core (مهم)
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough()
                       //.EnableTokenEndpointPassthrough()
                       //.EnableLogoutEndpointPassthrough()
                       .EnableStatusCodePagesIntegration()
                       ;

                // انتخاب فرمت توکن
                options.AllowRefreshTokenFlow();
            })
            .AddValidation(options =>
            {
                // اگر همین اپ خودش SSO Server و Resource Server است:
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }

    public static RsaSecurityKey CreateRsaSeruciryKey(IConfiguration config, IWebHostEnvironment env)
    {
        string encryptionKeyUrl = config.GetValue<string>("Oidc:EncryptionKey:Url")!;
        string keyPath = Path.Combine(env.MapPath(encryptionKeyUrl));

        string encryptionKeyPassword = config.GetValue<string>("Oidc:EncryptionKey:Password")!;

        var key = RSA.Create();
        key.ImportFromEncryptedPem(File.ReadAllText(keyPath), encryptionKeyPassword);
        return new RsaSecurityKey(key)!;
    }
}

public interface IOpenIddictServerService
{
    void AddOpenIddictConfig();
    RsaSecurityKey CreateRsaSeruciryKey();
}