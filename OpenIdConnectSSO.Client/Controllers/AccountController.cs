using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIdConnectSSO.Client.ViewModels;

namespace OpenIdConnectSSO.Client.Controllers;

public class AccountController(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    ILogger<AccountController> logger
    ) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(loginViewModel);
        }

        var user = await userManager.FindByNameAsync(loginViewModel.Username);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(loginViewModel);
        }

        var resutl = await signInManager.PasswordSignInAsync(
            userName: loginViewModel.Username,
            password: loginViewModel.Password,
            isPersistent: loginViewModel.IsPersistent,
            lockoutOnFailure: false);

        if (!resutl.Succeeded)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(loginViewModel);
        }

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }


    [AllowAnonymous]
    [HttpGet]
    public IActionResult LoginWithSso(string? returnUrl)
    {
        var redirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        var authProps = new AuthenticationProperties { RedirectUri = redirectUri };

        return Challenge(authProps, "oidc");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return LocalRedirect("/");
    }


    [HttpGet]
    public IActionResult SsoError(string type, string errorId) => View(new SsoErrorViewModel
    {
        Type = type,
        ErrorId = errorId
    });
}
