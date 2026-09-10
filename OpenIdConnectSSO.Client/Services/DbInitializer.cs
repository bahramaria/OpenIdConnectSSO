using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OpenIdConnectSSO.Client.Data;

namespace OpenIdConnectSSO.Client.Services;

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
        await db.Database.MigrateAsync();

        // Seed data...

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
