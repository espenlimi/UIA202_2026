using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Heimevernet.Web.Services;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation(
            "Development email to {Email}. Subject: {Subject}. Body: {Body}",
            email,
            subject,
            WebUtility.HtmlDecode(htmlMessage));
        return Task.CompletedTask;
    }
}
