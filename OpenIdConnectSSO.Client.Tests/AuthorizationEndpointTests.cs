using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIdConnectSSO.AuthServer.Controllers;
using OpenIddict.Abstractions;
using System.Security.Claims;
using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class AuthorizationEndpointTests
{
    [Fact]
    public void UserInfo_ReturnsOnlySubject_WhenNoOptionalScopesWereGranted()
    {
        var controller = CreateController("user-1");

        var result = controller.UserInfo();
        var objectResult = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<Dictionary<string, object>>(objectResult.Value);

        Assert.Equal("user-1", payload["sub"]);
        Assert.DoesNotContain("name", payload.Keys);
        Assert.DoesNotContain("email", payload.Keys);
        Assert.DoesNotContain("roles", payload.Keys);
    }

    [Fact]
    public void UserInfo_ReturnsProfileAndEmail_WhenScopesWereGranted()
    {
        var controller = CreateController(
            "user-1",
            (OpenIddictConstants.Claims.Name, "Admin"),
            (OpenIddictConstants.Claims.Email, "admin@example.test"));

        var principal = controller.HttpContext.User;
        principal.SetScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email);

        var result = controller.UserInfo();
        var payload = Assert.IsType<Dictionary<string, object>>(Assert.IsType<OkObjectResult>(result).Value);

        Assert.Equal("user-1", payload["sub"]);
        Assert.Equal("Admin", payload["name"]);
        Assert.Equal("admin@example.test", payload["email"]);
        Assert.DoesNotContain("roles", payload.Keys);
    }

    [Fact]
    public void UserInfo_ReturnsRoles_WhenRolesScopeWasGranted()
    {
        var controller = CreateController(
            "user-1",
            (OpenIddictConstants.Claims.Role, "Admin"),
            (OpenIddictConstants.Claims.Role, "Employee"));

        controller.HttpContext.User.SetScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Roles);

        var result = controller.UserInfo();
        var payload = Assert.IsType<Dictionary<string, object>>(Assert.IsType<OkObjectResult>(result).Value);
        var roles = Assert.IsType<string[]>(payload["roles"]);

        Assert.Equal(["Admin", "Employee"], roles);
    }

    private static AuthorizationController CreateController(
        string subject,
        params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, subject));

        foreach (var claim in claims)
        {
            identity.AddClaim(new Claim(claim.Type, claim.Value));
        }

        return new AuthorizationController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }
}
