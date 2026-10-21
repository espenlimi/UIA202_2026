using Heimevernet.Web.Controllers;
using Heimevernet.Web.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.Controllers;

public class AccountControllerTests
{
    [Fact]
    public void Login_Get_ReturnsViewWithReturnUrl()
    {
        var controller = CreateController();

        var result = controller.Login("/Resource");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LoginViewModel>(viewResult.Model);
        Assert.Equal("/Resource", model.ReturnUrl);
    }

    [Fact]
    public async Task Login_Post_WithInvalidModel_ReturnsTheEnteredModel()
    {
        var controller = CreateController();
        var model = new LoginViewModel();
        controller.ModelState.AddModelError(nameof(LoginViewModel.Email), "Email is required.");

        var result = await controller.Login(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public void ForgotPassword_Get_ReturnsEmptyModel()
    {
        var controller = CreateController();

        var result = controller.ForgotPassword();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ForgotPasswordViewModel>(viewResult.Model);
    }

    [Fact]
    public void Register_Get_ReturnsEmptyModel()
    {
        var controller = CreateController();

        var result = controller.Register();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<RegisterViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Register_Post_WithInvalidModel_ReturnsTheEnteredModel()
    {
        var controller = CreateController();
        var model = new RegisterViewModel();
        controller.ModelState.AddModelError(nameof(RegisterViewModel.Email), "Email is required.");

        var result = await controller.Register(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task Register_Post_CreatesConfirmedUserAndRedirectsToLogin()
    {
        var userManager = CreateSubstituteUserManager();
        userManager.CreateAsync(Arg.Any<IdentityUser>(), "Password123!").Returns(IdentityResult.Success);
        var controller = CreateController(userManager);
        var model = new RegisterViewModel
        {
            Email = "new.user@heimevernet.local",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await controller.Register(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Login), redirectResult.ActionName);
        await userManager.Received(1).CreateAsync(
            Arg.Is<IdentityUser>(user =>
                user.UserName == model.Email
                && user.Email == model.Email
                && user.EmailConfirmed),
            model.Password);
    }

    [Fact]
    public async Task ForgotPassword_Post_WithInvalidModel_ReturnsTheEnteredModel()
    {
        var controller = CreateController();
        var model = new ForgotPasswordViewModel();
        controller.ModelState.AddModelError(nameof(ForgotPasswordViewModel.Email), "Email is required.");

        var result = await controller.ForgotPassword(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public void ResetPassword_Get_WithoutCodeOrEmail_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = controller.ResetPassword();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Post_WithInvalidModel_ReturnsTheEnteredModel()
    {
        var controller = CreateController();
        var model = new ResetPasswordViewModel();
        controller.ModelState.AddModelError(nameof(ResetPasswordViewModel.Password), "Password is required.");

        var result = await controller.ResetPassword(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public void ResetPasswordConfirmation_ReturnsView()
    {
        var controller = CreateController();

        var result = controller.ResetPasswordConfirmation();

        Assert.IsType<ViewResult>(result);
    }

    private static AccountController CreateController(UserManager<IdentityUser>? userManager = null)
    {
        userManager ??= new UserManager<IdentityUser>(
            Substitute.For<IUserStore<IdentityUser>>(),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<IdentityUser>>.Instance);

        var signInManager = new SignInManager<IdentityUser>(
            userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>(),
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<IdentityUser>>.Instance,
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<IdentityUser>>());

        return new AccountController(
            userManager,
            signInManager,
            Substitute.For<IEmailSender>());
    }

    private static UserManager<IdentityUser> CreateSubstituteUserManager()
    {
        return Substitute.For<UserManager<IdentityUser>>(
            Substitute.For<IUserStore<IdentityUser>>(),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<IdentityUser>>.Instance);
    }
}
