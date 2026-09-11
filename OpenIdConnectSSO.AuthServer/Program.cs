using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OpenIdConnectSSO.AuthServer.Data;
using OpenIdConnectSSO.AuthServer.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;
var env = builder.Environment;

services.AddControllersWithViews();

services.AddDbContext<AppDbContext>(e =>
{
    e.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    e.UseOpenIddict();
});

services
    .AddIdentity<IdentityUser, IdentityRole>(e =>
    {
        e.User.RequireUniqueEmail = false;
        e.Password.RequiredLength = 6;
        e.Password.RequireDigit = false;
        e.Password.RequireLowercase = false;
        e.Password.RequireUppercase = false;
        e.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.AuthServer";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

services.AddScoped<IDbInitializer, DbInitializer>();
services.AddOpenIddictConfig(config, builder.Environment);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program
{
}
