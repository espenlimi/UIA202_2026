using System.ComponentModel.DataAnnotations;

namespace Heimevernet.Web.Models.ViewModels.Account;

public sealed class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
