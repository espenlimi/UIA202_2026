using Microsoft.AspNetCore.Identity;

namespace Heimevernet.Web.DataAccess;

public sealed class RepositoryIdentityUserStore(IIdentityUserRepository repository) :
    IUserPasswordStore<IdentityUser>,
    IUserEmailStore<IdentityUser>,
    IUserRoleStore<IdentityUser>,
    IUserSecurityStampStore<IdentityUser>,
    IUserLockoutStore<IdentityUser>
{
    private readonly Dictionary<IdentityUser, string?> originalConcurrencyStamps = new(ReferenceEqualityComparer.Instance);

    public void Dispose()
    {
        originalConcurrencyStamps.Clear();
    }

    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return CreateUserAsync(user, cancellationToken);
    }

    public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        if (!originalConcurrencyStamps.TryGetValue(user, out var originalConcurrencyStamp))
        {
            return IdentityResult.Failed(
                new IdentityError { Code = "ConcurrencyFailure", Description = "The original user version is unavailable." });
        }

        var nextConcurrencyStamp = Guid.NewGuid().ToString();
        user.ConcurrencyStamp = nextConcurrencyStamp;

        var updated = false;
        try
        {
            updated = await repository.UpdateUserAsync(user, originalConcurrencyStamp, cancellationToken);
        }
        finally
        {
            if (!updated)
            {
                user.ConcurrencyStamp = originalConcurrencyStamp;
            }
        }

        if (!updated)
        {
            return IdentityResult.Failed(
                new IdentityError { Code = "ConcurrencyFailure", Description = "The user could not be updated." });
        }

        originalConcurrencyStamps[user] = nextConcurrencyStamp;
        return IdentityResult.Success;
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return DeleteUserAsync(user, cancellationToken);
    }

    public async Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByIdAsync(userId, cancellationToken);
        TrackConcurrencyStamp(user);
        return user;
    }

    public async Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByNormalizedUserNameAsync(normalizedUserName, cancellationToken);
        TrackConcurrencyStamp(user);
        return user;
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash is not null);
    }

    public Task SetEmailAsync(IdentityUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<IdentityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        TrackConcurrencyStamp(user);
        return user;
    }

    public Task<string?> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(IdentityUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public async Task AddToRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = await repository.FindRoleByNormalizedNameAsync(normalizedRoleName, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{normalizedRoleName}' does not exist.");
        await repository.AddUserToRoleAsync(user.Id, role.Id, cancellationToken);
    }

    public async Task RemoveFromRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = await repository.FindRoleByNormalizedNameAsync(normalizedRoleName, cancellationToken);
        if (role is not null)
        {
            await repository.RemoveUserFromRoleAsync(user.Id, role.Id, cancellationToken);
        }
    }

    public Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return GetRolesAsyncCore(user, cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken)
    {
        return await repository.IsUserInRoleAsync(user.Id, normalizedRoleName, cancellationToken);
    }

    public Task<IList<IdentityUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return Task.FromResult<IList<IdentityUser>>([]);
    }

    public Task SetSecurityStampAsync(IdentityUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    private async Task<IdentityResult> CreateUserAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        await repository.CreateUserAsync(user, cancellationToken);
        originalConcurrencyStamps[user] = user.ConcurrencyStamp;
        return IdentityResult.Success;
    }

    private async Task<IdentityResult> DeleteUserAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        await repository.DeleteUserAsync(user, cancellationToken);
        return IdentityResult.Success;
    }

    private async Task<IList<string>> GetRolesAsyncCore(IdentityUser user, CancellationToken cancellationToken)
    {
        return (await repository.GetUserRoleNamesAsync(user.Id, cancellationToken)).ToList();
    }

    private void TrackConcurrencyStamp(IdentityUser? user)
    {
        if (user is not null)
        {
            originalConcurrencyStamps[user] = user.ConcurrencyStamp;
        }
    }
}
