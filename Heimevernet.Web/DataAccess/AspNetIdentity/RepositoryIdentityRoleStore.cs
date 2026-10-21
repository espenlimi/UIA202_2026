using Microsoft.AspNetCore.Identity;

namespace Heimevernet.Web.DataAccess;

public sealed class RepositoryIdentityRoleStore(IIdentityUserRepository repository) : IRoleStore<IdentityRole>
{
    private readonly Dictionary<string, string?> originalConcurrencyStamps = [];

    public void Dispose()
    {
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await repository.CreateRoleAsync(role, cancellationToken);
        originalConcurrencyStamps[role.Id] = role.ConcurrencyStamp;
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        if (!originalConcurrencyStamps.TryGetValue(role.Id, out var originalConcurrencyStamp))
        {
            return IdentityResult.Failed(
                new IdentityError { Code = "ConcurrencyFailure", Description = "The original role version is unavailable." });
        }

        var result = await repository.UpdateRoleAsync(role, originalConcurrencyStamp, cancellationToken)
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Code = "ConcurrencyFailure", Description = "The role could not be updated." });

        if (result.Succeeded)
        {
            originalConcurrencyStamps[role.Id] = role.ConcurrencyStamp;
        }

        return result;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await repository.DeleteRoleAsync(role, cancellationToken);
        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = await repository.FindRoleByIdAsync(roleId, cancellationToken);
        TrackConcurrencyStamp(role);
        return role;
    }

    public async Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = await repository.FindRoleByNormalizedNameAsync(normalizedRoleName, cancellationToken);
        TrackConcurrencyStamp(role);
        return role;
    }

    private void TrackConcurrencyStamp(IdentityRole? role)
    {
        if (role is not null)
        {
            originalConcurrencyStamps[role.Id] = role.ConcurrencyStamp;
        }
    }
}
