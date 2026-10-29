using Heimevernet.Web.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.Middleware;

public class ContentSecurityPolicyMiddlewareTests
{
    [Theory]
    [InlineData("Development", true)]
    [InlineData("Production", false)]
    public async Task InvokeAsync_SetsContentSecurityPolicyForEnvironment(
        string environmentName,
        bool isDevelopment)
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);
        var middleware = new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask, environment);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        var nonce = Assert.IsType<string>(context.Items[ContentSecurityPolicyMiddleware.NonceItemKey]);
        var policy = context.Response.Headers["Content-Security-Policy"].ToString();

        Assert.Equal(
            ContentSecurityPolicyMiddleware.CreatePolicy(nonce, isDevelopment),
            policy);
        Assert.Equal(
            $"script-src 'self' https://unpkg.com/leaflet@1.9.4/dist/leaflet.js 'nonce-{nonce}'",
            GetDirective(policy, "script-src"));
        Assert.Equal(
            "img-src 'self' data: https://unpkg.com/leaflet@1.9.4/dist/images/ https://tile.openstreetmap.org",
            GetDirective(policy, "img-src"));
        Assert.Equal(
            isDevelopment
                ? "connect-src 'self' https://unpkg.com/leaflet@1.9.4/dist/leaflet.js.map https://unpkg.com/leaflet@1.9.4/src/ http://localhost:* ws://localhost:*"
                : "connect-src 'self' https://unpkg.com/leaflet@1.9.4/dist/leaflet.js.map https://unpkg.com/leaflet@1.9.4/src/",
            GetDirective(policy, "connect-src"));
        Assert.Equal(
            isDevelopment ? "unload=self" : null,
            context.Response.Headers["Permissions-Policy"].FirstOrDefault());
    }

    private static string GetDirective(string policy, string directiveName) =>
        policy.Split("; ").Single(directive =>
            directive.StartsWith($"{directiveName} ", StringComparison.Ordinal));
}
