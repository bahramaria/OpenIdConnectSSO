using Microsoft.AspNetCore.Hosting;

namespace OpenIdConnectSSO.Common;

public static class PathExtensions
{
    public static string MapPath(this IWebHostEnvironment env, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return env.ContentRootPath;

        path = path.TrimStart('~', '/');

        return Path.Combine(env.ContentRootPath, path.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }
}