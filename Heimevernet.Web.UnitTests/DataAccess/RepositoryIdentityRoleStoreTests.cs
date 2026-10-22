using Heimevernet.Web.DataAccess;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.DataAccess;

public class RepositoryIdentityRoleStoreTests
{
    [Fact]
    public async Task UpdateAsync_RotatesConcurrencyStampAfterSuccessfulUpdate()
    {
        var repository = Substitute.For<IIdentityUserRepository>();
        repository.UpdateRoleAsync(Arg.Any<IdentityRole>(), Arg.Any<string?>(), CancellationToken.None).Returns(true);
        var store = new RepositoryIdentityRoleStore(repository);
        var role = new IdentityRole("Admin");
        var originalConcurrencyStamp = role.ConcurrencyStamp;
        await store.CreateAsync(role, CancellationToken.None);

        var firstResult = await store.UpdateAsync(role, CancellationToken.None);
        var firstUpdatedStamp = role.ConcurrencyStamp;
        var secondResult = await store.UpdateAsync(role, CancellationToken.None);

        Assert.True(firstResult.Succeeded);
        Assert.True(secondResult.Succeeded);
        Assert.NotEqual(originalConcurrencyStamp, firstUpdatedStamp);
        Assert.NotEqual(firstUpdatedStamp, role.ConcurrencyStamp);
        await repository.Received(1).UpdateRoleAsync(role, originalConcurrencyStamp, CancellationToken.None);
        await repository.Received(1).UpdateRoleAsync(role, firstUpdatedStamp, CancellationToken.None);
    }

    [Fact]
    public async Task UpdateAsync_TracksOriginalStampPerRoleInstance()
    {
        var repository = Substitute.For<IIdentityUserRepository>();
        var staleRole = new IdentityRole("Admin") { Id = "same-role-id", ConcurrencyStamp = "stale-stamp" };
        var currentRole = new IdentityRole("Admin") { Id = "same-role-id", ConcurrencyStamp = "current-stamp" };
        repository.FindRoleByIdAsync("same-role-id", CancellationToken.None)
            .Returns(staleRole, currentRole);
        var store = new RepositoryIdentityRoleStore(repository);
        var loadedStaleRole = Assert.IsType<IdentityRole>(
            await store.FindByIdAsync("same-role-id", CancellationToken.None));
        await store.FindByIdAsync("same-role-id", CancellationToken.None);

        var result = await store.UpdateAsync(loadedStaleRole, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("stale-stamp", loadedStaleRole.ConcurrencyStamp);
        await repository.Received(1).UpdateRoleAsync(
            loadedStaleRole,
            "stale-stamp",
            CancellationToken.None);
    }
}
