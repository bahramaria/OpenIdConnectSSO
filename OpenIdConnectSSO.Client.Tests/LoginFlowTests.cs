using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Identity;
using OpenIdConnectSSO.Client.Controllers;
using Xunit;

namespace OpenIdConnectSSO.Client.Tests;

public class LoginFlowTests
{
    [Fact]
    public void LoginWithSso_UsesOidcChallengeAndLocalReturnUrl()
    {
        var controller = CreateController();

        var result = controller.LoginWithSso("/Orders");

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal(["oidc"], challenge.AuthenticationSchemes);
        Assert.Equal("/Orders", challenge.Properties!.RedirectUri);
    }

    [Fact]
    public void LoginWithSso_UsesRootForExternalReturnUrl()
    {
        var controller = CreateController();

        var result = controller.LoginWithSso("https://example.test/redirect");

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/", challenge.Properties!.RedirectUri);
    }

    private static AccountController CreateController()
    {
        var httpContext = new DefaultHttpContext();
        var controller = new AccountController(
            signInManager: null!,
            userManager: null!,
            logger: NullLogger<AccountController>.Instance);

        controller.ControllerContext = new ControllerContext(
            new ActionContext(httpContext, new RouteData(), controller));

        return controller;
    }
}
