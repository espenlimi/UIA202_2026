using Heimevernet.Web.Middleware;
using Microsoft.AspNetCore.Http;

namespace Heimevernet.Web.UnitTests.Middleware;

public class ContentSecurityPolicyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SetsContentSecurityPolicyHeader()
    {
        var middleware = new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        var nonce = Assert.IsType<string>(context.Items[ContentSecurityPolicyMiddleware.NonceItemKey]);
        Assert.Equal(
            ContentSecurityPolicyMiddleware.CreatePolicy(nonce),
            context.Response.Headers["Content-Security-Policy"]);
    }
}
