using Microsoft.AspNetCore.Identity;

using OpenIdConnectSSO.Client.Data;

namespace OpenIdConnectSSO.Client.Services;

#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously.

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class DbInitializer(
    //UserManager<IdentityUser> userManager,
    //RoleManager<IdentityRole> roleManager,
    ClientAppDbContext db
    ) : IDbInitializer
{
    public async Task InitializeAsync()
    {
        //if (!await roleManager.RoleExistsAsync("Admin"))
        //{
        //    await roleManager.CreateAsync(new IdentityRole("Admin"));
        //}

        //if (await userManager.FindByNameAsync("Admin") == null)
        //{
        //    IdentityUser user = new() { UserName = "Admin" };
        //    await userManager.CreateAsync(user, "123456");
        //    await userManager.AddToRoleAsync(user, "Admin");
        //}
    }
}

#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
