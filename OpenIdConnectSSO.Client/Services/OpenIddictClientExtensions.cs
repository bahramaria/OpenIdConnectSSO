using Microsoft.IdentityModel.Tokens;

using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

using OpenIdConnectSSO.Common;

namespace OpenIdConnectSSO.Client.Services;

public static class OpenIddictClientExtensions
{
    public static AuthenticationBuilder AddOpenIddictConfig(this AuthenticationBuilder builder, IConfiguration config, IWebHostEnvironment env)
    { 
        return builder
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = config["Oidc:Authority"]
                    ?? throw new InvalidOperationException("Authority not found in appsettings.");

                options.ClientId = config["Oidc:ClientId"]
                    ?? throw new InvalidOperationException("ClientId not found in appsettings.");

                options.ClientSecret = config["Oidc:ClientSecret"]
                    ?? throw new InvalidOperationException("ClientSecret not found in appsettings.");

                options.ResponseType = config["Oidc:ResponseType"]
                    ?? throw new InvalidOperationException("ResponseType not found in appsettings.");

                var scopes = config["Oidc:Scopes"]?.Split(' ')
                    ?? throw new InvalidOperationException("ResponseType not found in appsettings.");

                foreach (var scope in scopes)
                {
                    options.Scope.Add(scope);
                }

                options.SaveTokens = true;

                options.GetClaimsFromUserInfoEndpoint = true;
                if (env.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }

                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();

                        var errorId = Guid.NewGuid().ToString("N");

                        context.Response.Redirect($"/Account/SsoError?type=remote&errorId={errorId}");

                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        context.HandleResponse();

                        var errorId = Guid.NewGuid().ToString("N");

                        context.Response.Redirect($"/Account/SsoError?type=auth&errorId={errorId}");

                        return Task.CompletedTask;
                    },

                    OnAccessDenied = context =>
                    {
                        context.HandleResponse();

                        context.Response.Redirect("/Account/Login?error=access_denied");

                        return Task.CompletedTask;
                    }
                };
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
