using Heimevernet.Web.Services;
using Microsoft.Extensions.Logging;

namespace Heimevernet.Web.UnitTests.Services;

public class ConsoleEmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_DecodesHtmlEntitiesForConsoleOutput()
    {
        var logger = new TestLogger<ConsoleEmailSender>();
        var sender = new ConsoleEmailSender(logger);

        await sender.SendEmailAsync(
            "admin@heimevernet.local",
            "Reset password",
            "<a href='https://example.test/reset?code=abc&amp;email=admin%40heimevernet.local'>Reset</a>");

        var message = Assert.Single(logger.Messages);
        Assert.Contains("code=abc&email=admin%40heimevernet.local", message);
        Assert.DoesNotContain("&amp;", message);
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
