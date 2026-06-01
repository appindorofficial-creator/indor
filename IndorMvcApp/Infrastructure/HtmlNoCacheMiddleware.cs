namespace IndorMvcApp.Infrastructure;

/// <summary>
/// Prevents WebViews from caching HTML responses (Razor views).
/// Static files under /css, /js, /lib keep their own cache headers.
/// </summary>
public sealed class HtmlNoCacheMiddleware
{
    private static readonly string[] StaticPrefixes = ["/css", "/js", "/lib", "/images", "/favicon"];

    private readonly RequestDelegate _next;

    public HtmlNoCacheMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (!IsStaticAsset(path))
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.CacheControl = "no-store, no-cache, must-revalidate";
                headers.Pragma = "no-cache";
                headers.Expires = "0";
                return Task.CompletedTask;
            });
        }

        await _next(context);
    }

    private static bool IsStaticAsset(string path)
    {
        foreach (var prefix in StaticPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return Path.HasExtension(path)
            && !path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
    }
}
