using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OpenIdConnectSSO.AuthServer.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected AppDbContext()
    {
    }
}
