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
        static IEnumerable<string> GetTokenDestinations(Claim _) =>
        [
            OpenIddictConstants.Destinations.AccessToken,
            OpenIddictConstants.Destinations.IdentityToken
        ];

        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request is missing.");

        var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        var user = authResult.Principal;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("The authenticated user does not have a name identifier claim.");
        }

        var scopes = request.GetScopes().ToArray();
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, userId)
                .SetDestinations(GetTokenDestinations);

        if (scopes.Contains(OpenIddictConstants.Scopes.Profile))
        {
            var userName = user.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                identity.AddClaim(OpenIddictConstants.Claims.Name, userName)
                        .SetDestinations(GetTokenDestinations);
            }
        }

        if (scopes.Contains(OpenIddictConstants.Scopes.Email))
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                identity.AddClaim(OpenIddictConstants.Claims.Email, email)
                        .SetDestinations(GetTokenDestinations);
            }
        }

        if (scopes.Contains(OpenIddictConstants.Scopes.Roles))
        {
            foreach (var role in user.FindAll(ClaimTypes.Role).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct())
            {
                identity.AddClaim(OpenIddictConstants.Claims.Role, role)
                        .SetDestinations(GetTokenDestinations);
            }
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);

        return SignIn(
            principal,
            new AuthenticationProperties(),
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("/connect/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public IActionResult UserInfo()
    {
        var subject = User.GetClaim(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Unauthorized();
        }

        var scopes = User.GetScopes();
        var result = new Dictionary<string, object>
        {
            [OpenIddictConstants.Claims.Subject] = subject
        };

        if (scopes.Contains(OpenIddictConstants.Scopes.Profile))
        {
            var name = User.GetClaim(OpenIddictConstants.Claims.Name);
            if (!string.IsNullOrWhiteSpace(name))
            {
                result[OpenIddictConstants.Claims.Name] = name;
            }
        }

        if (scopes.Contains(OpenIddictConstants.Scopes.Email))
        {
            var email = User.GetClaim(OpenIddictConstants.Claims.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                result[OpenIddictConstants.Claims.Email] = email;
            }
        }

        if (scopes.Contains(OpenIddictConstants.Scopes.Roles))
        {
            result["roles"] = User
                .FindAll(OpenIddictConstants.Claims.Role)
                .Select(claim => claim.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct()
                .ToArray();
        }

        return Ok(result);
    }
}
