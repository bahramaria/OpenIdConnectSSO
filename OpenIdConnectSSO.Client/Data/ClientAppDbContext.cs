using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OpenIdConnectSSO.Client.Data;

public class ClientAppDbContext : IdentityDbContext
{
    public ClientAppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected ClientAppDbContext()
    {
    }
}
