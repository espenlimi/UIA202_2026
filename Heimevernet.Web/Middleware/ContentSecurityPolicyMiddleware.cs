using System.Security.Cryptography;

namespace Heimevernet.Web.Middleware;

public sealed class ContentSecurityPolicyMiddleware(
    RequestDelegate next,
    IWebHostEnvironment environment)
{
    public const string NonceItemKey = "CspNonce";

    public static string CreatePolicy(string nonce, bool isDevelopment = false)
    {
        var connectSource = isDevelopment
            ? "connect-src 'self' https://unpkg.com/leaflet@1.9.4/dist/leaflet.js.map https://unpkg.com/leaflet@1.9.4/src/ http://localhost:* ws://localhost:*"
            : "connect-src 'self' https://unpkg.com/leaflet@1.9.4/dist/leaflet.js.map https://unpkg.com/leaflet@1.9.4/src/";

        return string.Join("; ", new[]
        {
            "default-src 'self'", // Default source for resource types without a specific directive.
            "base-uri 'self'", // Restricts URLs allowed in the document's <base> element.
            "object-src 'none'", // Blocks embedded plug-in content such as <object> and <embed>.
            "frame-ancestors 'none'", // Prevents this site from being embedded in frames.
            "form-action 'self'", // Restricts form submissions to this site.
            $"script-src 'self' https://unpkg.com/leaflet@1.9.4/dist/leaflet.js 'nonce-{nonce}'", // Allows local scripts, the pinned Leaflet script, and inline scripts carrying this response's nonce.
            "style-src 'self' https://unpkg.com", // Allows local and Leaflet CDN stylesheets.
            "img-src 'self' data: https://unpkg.com/leaflet@1.9.4/dist/images/ https://tile.openstreetmap.org", // Allows local images, Leaflet icons, and map tiles.
            "font-src 'self' data:", // Allows local fonts and embedded data fonts.
            connectSource // Allows the Leaflet source map and source files, plus BrowserLink's dynamic localhost endpoints in development.
        });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items[NonceItemKey] = nonce;
        var isDevelopment = environment.IsDevelopment();
        var policy = CreatePolicy(nonce, isDevelopment);

        context.Response.Headers["Content-Security-Policy"] = policy;
        if (isDevelopment)
        {
            context.Response.Headers["Permissions-Policy"] = "unload=self";
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Content-Security-Policy"] = policy;
            if (isDevelopment)
            {
                context.Response.Headers["Permissions-Policy"] = "unload=self";
            }

            return Task.CompletedTask;
        });
        await next(context);
    }
}
