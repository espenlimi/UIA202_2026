using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Heimevernet.Web.DataAccess;

public sealed class DapperIdentityUserRepository(IResourceDbConnectionFactory connectionFactory) : IIdentityUserRepository
{
    public async Task CreateUserAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
                LockoutEnd, LockoutEnabled, AccessFailedCount)
            VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled,
                @LockoutEnd, @LockoutEnabled, @AccessFailedCount)
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateUserAsync(
        IdentityUser user,
        string? originalConcurrencyStamp,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE AspNetUsers SET UserName = @UserName, NormalizedUserName = @NormalizedUserName, Email = @Email,
                NormalizedEmail = @NormalizedEmail, EmailConfirmed = @EmailConfirmed, PasswordHash = @PasswordHash,
                SecurityStamp = @SecurityStamp, ConcurrencyStamp = @ConcurrencyStamp, PhoneNumber = @PhoneNumber,
                PhoneNumberConfirmed = @PhoneNumberConfirmed, TwoFactorEnabled = @TwoFactorEnabled, LockoutEnd = @LockoutEnd,
                LockoutEnabled = @LockoutEnabled, AccessFailedCount = @AccessFailedCount
            WHERE Id = @Id AND (ConcurrencyStamp = @OriginalConcurrencyStamp
                OR (ConcurrencyStamp IS NULL AND @OriginalConcurrencyStamp IS NULL))
            """;
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters(user);
        parameters.Add("OriginalConcurrencyStamp", originalConcurrencyStamp);
        return await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)) == 1;
    }

    public async Task DeleteUserAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM AspNetUsers WHERE Id = @Id", new { user.Id }, cancellationToken: cancellationToken));
    }

    public Task<IdentityUser?> FindUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return FindUserAsync("Id = @Value", userId, cancellationToken);
    }

    public Task<IdentityUser?> FindUserByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return FindUserAsync("NormalizedUserName = @Value", normalizedUserName, cancellationToken);
    }

    public Task<IdentityUser?> FindUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return FindUserAsync("NormalizedEmail = @Value", normalizedEmail, cancellationToken);
    }

    public async Task CreateRoleAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            "INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)",
            role, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateRoleAsync(
        IdentityRole role,
        string? originalConcurrencyStamp,
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters(role);
        parameters.Add("OriginalConcurrencyStamp", originalConcurrencyStamp);
        return await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE AspNetRoles SET Name = @Name, NormalizedName = @NormalizedName, ConcurrencyStamp = @ConcurrencyStamp WHERE Id = @Id AND (ConcurrencyStamp = @OriginalConcurrencyStamp OR (ConcurrencyStamp IS NULL AND @OriginalConcurrencyStamp IS NULL))",
            parameters, cancellationToken: cancellationToken)) == 1;
    }

    public async Task DeleteRoleAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM AspNetRoles WHERE Id = @Id", new { role.Id }, cancellationToken: cancellationToken));
    }

    public Task<IdentityRole?> FindRoleByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return FindRoleAsync("Id = @Value", roleId, cancellationToken);
    }

    public Task<IdentityRole?> FindRoleByNormalizedNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return FindRoleAsync("NormalizedName = @Value", normalizedRoleName, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetUserRoleNamesAsync(string userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT r.Name FROM AspNetRoles r
            INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
            WHERE ur.UserId = @UserId
            """;
        using var connection = connectionFactory.CreateConnection();
        var roles = await connection.QueryAsync<string>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return roles.ToList().AsReadOnly();
    }

    public async Task<bool> IsUserInRoleAsync(
        string userId,
        string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1 FROM AspNetRoles r
                INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
                WHERE ur.UserId = @UserId AND r.NormalizedName = @NormalizedRoleName
            )
            """;
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { UserId = userId, NormalizedRoleName = normalizedRoleName },
            cancellationToken: cancellationToken));
    }

    public async Task AddUserToRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            "INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
            new { UserId = userId, RoleId = roleId }, cancellationToken: cancellationToken));
    }

    public async Task RemoveUserFromRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId",
            new { UserId = userId, RoleId = roleId }, cancellationToken: cancellationToken));
    }

    private async Task<IdentityUser?> FindUserAsync(string predicate, string value, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityUser>(new CommandDefinition(
            $"SELECT * FROM AspNetUsers WHERE {predicate}", new { Value = value }, cancellationToken: cancellationToken));
    }

    private async Task<IdentityRole?> FindRoleAsync(string predicate, string value, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityRole>(new CommandDefinition(
            $"SELECT * FROM AspNetRoles WHERE {predicate}", new { Value = value }, cancellationToken: cancellationToken));
    }
}
