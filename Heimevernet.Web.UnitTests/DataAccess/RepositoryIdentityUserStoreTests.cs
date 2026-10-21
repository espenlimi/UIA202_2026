using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.DataAccess;

public class RepositoryIdentityUserStoreTests
{
    [Fact]
    public async Task CreateAsync_DelegatesTheUserToRepository()
    {
        var repository = Substitute.For<IIdentityUserRepository>();
        var store = new RepositoryIdentityUserStore(repository);
        var user = new IdentityUser("student@example.test");

        var result = await store.CreateAsync(user, CancellationToken.None);

        Assert.True(result.Succeeded);
        await repository.Received(1).CreateUserAsync(user, CancellationToken.None);
    }

    [Fact]
    public async Task AddToRoleAsync_UsesTheRoleIdFromRepository()
    {
        var repository = Substitute.For<IIdentityUserRepository>();
        var normalizedAdminRole = RoleNames.Admin.ToUpperInvariant();
        var role = new IdentityRole(RoleNames.Admin) { NormalizedName = normalizedAdminRole };
        repository.FindRoleByNormalizedNameAsync(normalizedAdminRole, CancellationToken.None).Returns(role);
        var store = new RepositoryIdentityUserStore(repository);
        var user = new IdentityUser();

        await store.AddToRoleAsync(user, normalizedAdminRole, CancellationToken.None);

        await repository.Received(1).AddUserToRoleAsync(user.Id, role.Id, CancellationToken.None);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsConcurrencyFailureWhenRepositoryCannotUpdateUser()
    {
        var repository = Substitute.For<IIdentityUserRepository>();
        var store = new RepositoryIdentityUserStore(repository);
        var user = new IdentityUser();
        var originalConcurrencyStamp = user.ConcurrencyStamp;
        await store.CreateAsync(user, CancellationToken.None);
        user.ConcurrencyStamp = "updated-concurrency-stamp";
        repository.UpdateUserAsync(user, originalConcurrencyStamp, CancellationToken.None).Returns(false);

        var result = await store.UpdateAsync(user, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "ConcurrencyFailure");
        await repository.Received(1).UpdateUserAsync(user, originalConcurrencyStamp, CancellationToken.None);
    }

    [Fact]
    public async Task LockoutStore_TracksFailedLoginAttempts()
    {
        var store = new RepositoryIdentityUserStore(Substitute.For<IIdentityUserRepository>());
        var lockoutStore = Assert.IsAssignableFrom<IUserLockoutStore<IdentityUser>>(store);
        var user = new IdentityUser { LockoutEnabled = true };

        var failedAttempts = await lockoutStore.IncrementAccessFailedCountAsync(user, CancellationToken.None);
        await lockoutStore.SetLockoutEndDateAsync(
            user,
            DateTimeOffset.UtcNow.AddMinutes(15),
            CancellationToken.None);

        Assert.Equal(1, failedAttempts);
        Assert.True(await lockoutStore.GetLockoutEnabledAsync(user, CancellationToken.None));
        Assert.NotNull(await lockoutStore.GetLockoutEndDateAsync(user, CancellationToken.None));
    }
}
