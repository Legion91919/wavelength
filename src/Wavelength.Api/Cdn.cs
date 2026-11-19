using Microsoft.Net.Http.Headers;

namespace Wavelength.Api;

/// <summary>
/// Serves audio from wwwroot/cdn with long-lived immutable caching, the way a CDN edge would.
/// In production the same paths sit behind a real CDN and never reach the API servers.
/// </summary>
internal static class Cdn
{
    public static WebApplication UseCdnStaticFiles(this WebApplication app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                if (ctx.Context.Request.Path.StartsWithSegments("/cdn", StringComparison.Ordinal))
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";
                    ctx.Context.Response.Headers[HeaderNames.AccessControlAllowOrigin] = "*";
                }
            },
        });
        return app;
    }
}
