using System.Security.Cryptography;

namespace Heimevernet.Web.Middleware;

public sealed class ContentSecurityPolicyMiddleware(RequestDelegate next)
{
    public const string NonceItemKey = "CspNonce";

    public static string CreatePolicy(string nonce) =>
        string.Join("; ", new[]
        {
            "default-src 'self'", // Default source for resource types without a specific directive.
            "base-uri 'self'", // Restricts URLs allowed in the document's <base> element.
            "object-src 'none'", // Blocks embedded plug-in content such as <object> and <embed>.
            "frame-ancestors 'none'", // Prevents this site from being embedded in frames.
            "form-action 'self'", // Restricts form submissions to this site.
            $"script-src 'self' https://unpkg.com 'nonce-{nonce}'", // Allows local/CDN scripts and inline scripts carrying this response's nonce; blocks other inline scripts.
            "style-src 'self' https://unpkg.com", // Allows local and Leaflet CDN stylesheets.
            "img-src 'self' data: https://unpkg.com https://tile.openstreetmap.org", // Allows local images, Leaflet icons, and map tiles.
            "font-src 'self' data:", // Allows local fonts and embedded data fonts.
            "connect-src 'self'" // Restricts fetch, XHR, and WebSocket connections to this site.
        });

    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items[NonceItemKey] = nonce;
        var policy = CreatePolicy(nonce);

        context.Response.Headers["Content-Security-Policy"] = policy;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Content-Security-Policy"] = policy;
            return Task.CompletedTask;
        });
        await next(context);
    }
}
