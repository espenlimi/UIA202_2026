using Heimevernet.Web.Services;
using Heimevernet.Web.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.Services;

public class IdentitySeederTests
{
    [Fact]
    public async Task SeedRolesAsync_CreatesAdminRoleWhenItDoesNotExist()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        store.FindByNameAsync(RoleNames.Admin.ToUpperInvariant(), Arg.Any<CancellationToken>()).Returns((IdentityRole?)null);
        store.CreateAsync(Arg.Any<IdentityRole>(), Arg.Any<CancellationToken>()).Returns(IdentityResult.Success);
        var roleManager = CreateRoleManager(store);

        await IdentitySeeder.SeedRolesAsync(roleManager);

        await store.Received(1).CreateAsync(
            Arg.Is<IdentityRole>(role => role.Name == RoleNames.Admin),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedRolesAsync_DoesNotCreateAdminRoleWhenItAlreadyExists()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        store.FindByNameAsync(RoleNames.Admin.ToUpperInvariant(), Arg.Any<CancellationToken>())
            .Returns(new IdentityRole(RoleNames.Admin));
        var roleManager = CreateRoleManager(store);

        await IdentitySeeder.SeedRolesAsync(roleManager);

        await store.DidNotReceive().CreateAsync(Arg.Any<IdentityRole>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedDevelopmentAdminAsync_CreatesPasswordlessAdminUserAndAssignsAdminRole()
    {
        var userManager = CreateUserManager();
        userManager.FindByEmailAsync(IdentitySeeder.DevelopmentAdminEmail).Returns((IdentityUser?)null);
        userManager.CreateAsync(Arg.Any<IdentityUser>()).Returns(IdentityResult.Success);
        userManager.IsInRoleAsync(Arg.Any<IdentityUser>(), RoleNames.Admin).Returns(false);
        userManager.AddToRoleAsync(Arg.Any<IdentityUser>(), RoleNames.Admin).Returns(IdentityResult.Success);

        await IdentitySeeder.SeedDevelopmentAdminAsync(userManager);

        await userManager.Received(1).CreateAsync(
            Arg.Is<IdentityUser>(user =>
                user.UserName == IdentitySeeder.DevelopmentAdminEmail
                && user.Email == IdentitySeeder.DevelopmentAdminEmail
                && user.EmailConfirmed));
        await userManager.Received(1).AddToRoleAsync(Arg.Any<IdentityUser>(), RoleNames.Admin);
    }

    [Fact]
    public async Task SeedDevelopmentAdminAsync_DoesNotModifyExistingAdminPassword()
    {
        var user = new IdentityUser(IdentitySeeder.DevelopmentAdminEmail)
        {
            Email = IdentitySeeder.DevelopmentAdminEmail
        };
        var userManager = CreateUserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.IsInRoleAsync(user, RoleNames.Admin).Returns(true);

        await IdentitySeeder.SeedDevelopmentAdminAsync(userManager);

        await userManager.DidNotReceive().CreateAsync(Arg.Any<IdentityUser>());
        await userManager.DidNotReceive().AddToRoleAsync(user, Arg.Any<string>());
    }

    [Fact]
    public async Task SeedDevelopmentUserAsync_CreatesPasswordlessUserWithoutRoles()
    {
        var userManager = CreateUserManager();
        userManager.FindByEmailAsync(IdentitySeeder.DevelopmentUserEmail).Returns((IdentityUser?)null);
        userManager.CreateAsync(Arg.Any<IdentityUser>()).Returns(IdentityResult.Success);

        var user = await IdentitySeeder.SeedDevelopmentUserAsync(userManager);

        Assert.Equal(IdentitySeeder.DevelopmentUserEmail, user.Email);
        Assert.True(user.EmailConfirmed);
        await userManager.Received(1).CreateAsync(user);
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<IdentityUser>(), Arg.Any<string>());
    }

    private static RoleManager<IdentityRole> CreateRoleManager(IRoleStore<IdentityRole> store)
    {
        return new RoleManager<IdentityRole>(
            store,
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<IdentityRole>>.Instance);
    }

    private static UserManager<IdentityUser> CreateUserManager()
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
