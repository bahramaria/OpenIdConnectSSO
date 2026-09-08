namespace OpenIdConnectSSO.Client;

public static class SsoErrorHandlingMiddlewareExtensionMethod
{
    public static IApplicationBuilder UseSsoErrorHandling(this WebApplication? application)
    {
        return application!.UseMiddleware<SsoErrorHandlingMiddleware>();
    }
}

public class SsoErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<SsoErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid().ToString("N");

            logger.LogError(ex, "SSO Error Occurred. ErrorId: {ErrorId}", errorId);

            context.Response.Redirect($"/Account/SsoError?type=system&errorId={errorId}");
        }
    }
}
