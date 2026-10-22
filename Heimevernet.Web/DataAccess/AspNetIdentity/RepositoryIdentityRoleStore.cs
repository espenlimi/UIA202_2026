using Microsoft.AspNetCore.Identity;

namespace Heimevernet.Web.DataAccess;

public sealed class RepositoryIdentityRoleStore(IIdentityUserRepository repository) : IRoleStore<IdentityRole>
{
    private readonly Dictionary<IdentityRole, string?> originalConcurrencyStamps = new(ReferenceEqualityComparer.Instance);

    public void Dispose()
    {
        originalConcurrencyStamps.Clear();
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await repository.CreateRoleAsync(role, cancellationToken);
        originalConcurrencyStamps[role] = role.ConcurrencyStamp;
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        if (!originalConcurrencyStamps.TryGetValue(role, out var originalConcurrencyStamp))
        {
            return IdentityResult.Failed(
                new IdentityError { Code = "ConcurrencyFailure", Description = "The original role version is unavailable." });
        }

        var nextConcurrencyStamp = Guid.NewGuid().ToString();
        role.ConcurrencyStamp = nextConcurrencyStamp;

        var updated = false;
        try
        {
            updated = await repository.UpdateRoleAsync(role, originalConcurrencyStamp, cancellationToken);
        }
        finally
        {
            if (!updated)
            {
                role.ConcurrencyStamp = originalConcurrencyStamp;
            }
        }

        if (!updated)
        {
            return IdentityResult.Failed(
                new IdentityError { Code = "ConcurrencyFailure", Description = "The role could not be updated." });
        }

        originalConcurrencyStamps[role] = nextConcurrencyStamp;
        return IdentityResult.Success;
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
            originalConcurrencyStamps[role] = role.ConcurrencyStamp;
        }
    }
}
