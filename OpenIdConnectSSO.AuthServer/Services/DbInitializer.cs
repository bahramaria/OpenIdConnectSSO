using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OpenIdConnectSSO.AuthServer.Data;

using OpenIddict.Abstractions;

namespace OpenIdConnectSSO.AuthServer.Services;

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class DbInitializer(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext db,
    IConfiguration config
    ) : IDbInitializer
{
    public async Task InitializeAsync()
    {
        if (db.Database.IsRelational() && (await db.Database.GetPendingMigrationsAsync()).Any())
        {
            await db.Database.MigrateAsync();
        }

        await SeedUsersAsync();
        await SeedOpenIdAsync();
    }

    private async Task SeedUsersAsync()
    {
        await CreateRoleAsync(
            role: "Admin",
            sampleUsername: "Admin",
            samplePassword: GetRequiredConfiguration("Seed:Users:AdminPassword"));

        await CreateRoleAsync(
            role: "Employee",
            sampleUsername: "Employee1",
            samplePassword: GetRequiredConfiguration("Seed:Users:EmployeePassword"));
    }

    private async Task SeedOpenIdAsync()
    {
        await CreateScopesAsync();
        await CreateClientsAsync();
    }


    // ********** Seed Users *************************************************

    private async Task CreateRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task CreateRoleAsync(string role, string sampleUsername, string samplePassword)
    {
        if (string.IsNullOrWhiteSpace(sampleUsername))
        {
            throw new ArgumentNullException(nameof(sampleUsername));
        }

        if (string.IsNullOrWhiteSpace(samplePassword))
        {
            throw new ArgumentNullException(nameof(samplePassword));
        }

        await CreateRoleAsync(role);

        if (await userManager.FindByNameAsync(sampleUsername) == null)
        {
            IdentityUser user = new() { UserName = sampleUsername };
            await userManager.CreateAsync(user, samplePassword);
            await userManager.AddToRoleAsync(user, role);
        }
    }

    // ********** Seed OpenID ************************************************

    private async Task CreateScopesAsync()
    {
        List<string> scopes = ["openid", "profile", "email", "roles"];

        foreach (var scope in scopes)
        {
            if (await scopeManager.FindByNameAsync(scope) == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = scope });
            }
        }
    }

    private async Task CreateClientsAsync()
    {
        await CreateClientAsync(
            clientId: "sampleclient",
            clientSecret: GetRequiredConfiguration("Seed:Clients:SampleClientSecret"),
            displayName: "Sample Client",
            replace: false);

        await CreateClientAsync(
            clientId: "gateway_client_id",
            clientSecret: GetRequiredConfiguration("Seed:Clients:GatewayClientSecret"),
            displayName: "Gateway Client",
            redirectUri: "https://localhost:7002/getway-test.html",
            replace: false);
    }

    public async Task CreateClientAsync(
        string clientId,
        string clientSecret,
        string displayName,
        string redirectUri = "https://localhost:7002/signin-oidc",
        bool replace = false)
    {
        var authClient = await applicationManager.FindByClientIdAsync(clientId);
        if (authClient != null)
        {
            if (!replace)
                return;

            await applicationManager.DeleteAsync(authClient);
        }

        OpenIddictApplicationDescriptor descriptor = new()
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = displayName,
            RedirectUris = { new Uri(redirectUri) },
            PostLogoutRedirectUris = { new Uri("https://localhost:7002/signout-callback-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                "scp:openid",
                "scp:profile",
                "scp:email",
                "scp:roles"
            }
        };

        await applicationManager.CreateAsync(descriptor);
    }

    private string GetRequiredConfiguration(string key)
    {
        return config[key] ?? throw new InvalidOperationException(
            $"Required configuration '{key}' was not found. Configure it using user secrets or environment variables.");
    }
}
