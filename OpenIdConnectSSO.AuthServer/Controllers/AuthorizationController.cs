using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;

using System.Security.Claims;

namespace OpenIdConnectSSO.AuthServer.Controllers;

[ApiController]
public class AuthorizationController : Controller
{
    [HttpGet("/connect/authorize")]
    [Authorize]
    public async Task<IActionResult> Authorize()
    {
        static IEnumerable<string> selector(Claim _) =>
        [
            OpenIddictConstants.Destinations.AccessToken,
            OpenIddictConstants.Destinations.IdentityToken
        ];


        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request is missing.");

        var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var user = authResult.Principal!;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userName = user.Identity!.Name ?? "";

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        var scopes = request.GetScopes();

        // Subject
        identity.AddClaim(OpenIddictConstants.Claims.Subject, userId)
                .SetDestinations(selector);

        // Profile
        if (scopes.Contains(OpenIddictConstants.Scopes.Profile))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Name, userName)
                    .SetDestinations(selector);
        }

        // Email
        if (scopes.Contains(OpenIddictConstants.Scopes.Email))
        {
            var email = user.FindFirstValue(ClaimTypes.Email) ?? "";
            identity.AddClaim(OpenIddictConstants.Claims.Email, email)
                    .SetDestinations(selector);
        }

        // Roles
        if (scopes.Contains(OpenIddictConstants.Scopes.Roles))
        {
            foreach (var role in user.FindAll(ClaimTypes.Role))
            {
                identity.AddClaim(OpenIddictConstants.Claims.Role, role.Value)
                        .SetDestinations(selector);
            }
        }

        var finalPrincipal = new ClaimsPrincipal(identity);
        finalPrincipal.SetScopes(scopes);

        var ticket = new AuthenticationTicket(
            finalPrincipal,
            new AuthenticationProperties(),
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
    }


    [HttpGet("/connect/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public IActionResult UserInfo()
    {
        if (User == null)
        {
            return Unauthorized();
        }

        var scopes = User.GetScopes();

        var result = new Dictionary<string, object>
        {
            ["sub"] = User.GetClaim(OpenIddictConstants.Claims.Subject)!,
        };

        if (scopes.Contains(OpenIddictConstants.Scopes.Profile))
        {
            result["name"] = User.GetClaim(OpenIddictConstants.Claims.Name)!;
        }

        if (scopes.Contains(OpenIddictConstants.Scopes.Email))
        {
            result["email"] = User.GetClaim(OpenIddictConstants.Claims.Email)!;
        }

        if (scopes.Contains(OpenIddictConstants.Scopes.Roles))
        {
            result["roles"] = User.FindAll("role").Select(x => x.Value).ToArray();
        }

        return Ok(result);
    }
}
