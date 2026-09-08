using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OpenIdConnectSSO.Client;
using OpenIdConnectSSO.Client.Data;
using OpenIdConnectSSO.Client.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;
var env = builder.Environment;

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ClientAppDbContext>(e =>
{
    e.UseSqlServer(config.GetConnectionString("DefaultConnection"));
});

builder.Services
    .AddIdentityCore<IdentityUser>(e =>
    {
        e.User.RequireUniqueEmail = false;
        e.Password.RequiredLength = 6;
        e.Password.RequireNonAlphanumeric = false;
        e.Password.RequireDigit = false;
        e.Password.RequireLowercase = false;
        e.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ClientAppDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<IdentityUser>>()
    .AddUserManager<UserManager<IdentityUser>>()
    .AddRoleManager<RoleManager<IdentityRole>>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddOpenIddictConfig(config, env)
    .AddIdentityCookies();

builder.Services.AddAuthorization();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClientAppDbContext>();

    await db.Database.MigrateAsync();

    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeAsync();
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSsoErrorHandling();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
