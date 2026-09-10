using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Identity;
using OpenIdConnectSSO.Client.Controllers;

namespace OpenIdConnectSSO.Client.Tests;

public class LoginFlowTests
{
    [Fact]
    public void LoginWithSso_UsesOidcChallengeAndLocalReturnUrl()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = controller.LoginWithSso("/Orders");

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal(["oidc"], challenge.AuthenticationSchemes);
        Assert.Equal("/Orders", challenge.Properties!.RedirectUri);
    }

    [Fact]
    public void LoginWithSso_UsesRootForExternalReturnUrl()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = controller.LoginWithSso("https://example.test/redirect");

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/", challenge.Properties!.RedirectUri);
    }

    private static AccountController CreateController() =>
        new(
            signInManager: null!,
            userManager: null!,
            logger: NullLogger<AccountController>.Instance);
}
