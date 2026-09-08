using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIdConnectSSO.AuthServer.ViewModels;

namespace OpenIdConnectSSO.AuthServer.Controllers;

public class AccountController(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager
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
        if (user == null)
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
            ? (IActionResult)LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return LocalRedirect("/");
    }
}
