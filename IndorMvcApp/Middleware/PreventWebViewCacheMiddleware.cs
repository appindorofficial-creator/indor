namespace IndorMvcApp.Middleware;

/// <summary>
/// Prevents WebViews and mobile browsers from serving stale HTML for dynamic MVC pages.
/// Static files (css/js/lib) are served earlier and are not affected.
/// </summary>
public sealed class PreventWebViewCacheMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (ShouldPreventCache(context.Response.ContentType))
            {
                var headers = context.Response.Headers;
                headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
                headers.Pragma = "no-cache";
                headers.Expires = "0";
            }

            return Task.CompletedTask;
        });

        await next(context);
    }

    private static bool ShouldPreventCache(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType)
        && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);
}
