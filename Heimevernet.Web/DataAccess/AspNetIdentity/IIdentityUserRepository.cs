using Microsoft.AspNetCore.Identity;

namespace Heimevernet.Web.DataAccess;

public interface IIdentityUserRepository
{
    Task CreateUserAsync(IdentityUser user, CancellationToken cancellationToken);
    Task<bool> UpdateUserAsync(IdentityUser user, string? originalConcurrencyStamp, CancellationToken cancellationToken);
    Task DeleteUserAsync(IdentityUser user, CancellationToken cancellationToken);
    Task<IdentityUser?> FindUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task<IdentityUser?> FindUserByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken);
    Task<IdentityUser?> FindUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task CreateRoleAsync(IdentityRole role, CancellationToken cancellationToken);
    Task<bool> UpdateRoleAsync(IdentityRole role, string? originalConcurrencyStamp, CancellationToken cancellationToken);
    Task DeleteRoleAsync(IdentityRole role, CancellationToken cancellationToken);
    Task<IdentityRole?> FindRoleByIdAsync(string roleId, CancellationToken cancellationToken);
    Task<IdentityRole?> FindRoleByNormalizedNameAsync(string normalizedRoleName, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetUserRoleNamesAsync(string userId, CancellationToken cancellationToken);
    Task AddUserToRoleAsync(string userId, string roleId, CancellationToken cancellationToken);
    Task RemoveUserFromRoleAsync(string userId, string roleId, CancellationToken cancellationToken);
}
