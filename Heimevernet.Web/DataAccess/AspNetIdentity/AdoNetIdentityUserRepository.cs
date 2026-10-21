using System.Data;
using Microsoft.AspNetCore.Identity;

namespace Heimevernet.Web.DataAccess;

public sealed class AdoNetIdentityUserRepository(IResourceDbConnectionFactory connectionFactory) : IIdentityUserRepository
{
    public Task CreateUserAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash,
                SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount)
            VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PasswordHash,
                @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount)
            """;
        Execute(sql, CreateUserParameters(user));
        return Task.CompletedTask;
    }

    public Task<bool> UpdateUserAsync(
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
        var parameters = CreateUserParameters(user)
            .Append(("@OriginalConcurrencyStamp", originalConcurrencyStamp));
        return Task.FromResult(Execute(sql, parameters) == 1);
    }

    public Task DeleteUserAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        Execute("DELETE FROM AspNetUsers WHERE Id = @Id", [("@Id", user.Id)]);
        return Task.CompletedTask;
    }

    public Task<IdentityUser?> FindUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(FindUser("Id = @Value", userId));
    }

    public Task<IdentityUser?> FindUserByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return Task.FromResult(FindUser("NormalizedUserName = @Value", normalizedUserName));
    }

    public Task<IdentityUser?> FindUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return Task.FromResult(FindUser("NormalizedEmail = @Value", normalizedEmail));
    }

    public Task CreateRoleAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        Execute("INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)",
            [("@Id", role.Id), ("@Name", role.Name), ("@NormalizedName", role.NormalizedName), ("@ConcurrencyStamp", role.ConcurrencyStamp)]);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateRoleAsync(
        IdentityRole role,
        string? originalConcurrencyStamp,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Execute(
            "UPDATE AspNetRoles SET Name = @Name, NormalizedName = @NormalizedName, ConcurrencyStamp = @ConcurrencyStamp WHERE Id = @Id AND (ConcurrencyStamp = @OriginalConcurrencyStamp OR (ConcurrencyStamp IS NULL AND @OriginalConcurrencyStamp IS NULL))",
            [("@Id", role.Id), ("@Name", role.Name), ("@NormalizedName", role.NormalizedName), ("@ConcurrencyStamp", role.ConcurrencyStamp), ("@OriginalConcurrencyStamp", originalConcurrencyStamp)]) == 1);
    }

    public Task DeleteRoleAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        Execute("DELETE FROM AspNetRoles WHERE Id = @Id", [("@Id", role.Id)]);
        return Task.CompletedTask;
    }

    public Task<IdentityRole?> FindRoleByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return Task.FromResult(FindRole("Id = @Value", roleId));
    }

    public Task<IdentityRole?> FindRoleByNormalizedNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return Task.FromResult(FindRole("NormalizedName = @Value", normalizedRoleName));
    }

    public Task<IReadOnlyCollection<string>> GetUserRoleNamesAsync(string userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT r.NormalizedName FROM AspNetRoles r
            INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
            WHERE ur.UserId = @UserId
            """;
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, sql, [("@UserId", userId)]);
        using var reader = command.ExecuteReader();
        var roles = new List<string>();
        while (reader.Read())
        {
            roles.Add(reader.GetString(0));
        }

        return Task.FromResult<IReadOnlyCollection<string>>(roles.AsReadOnly());
    }

    public Task AddUserToRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
    {
        Execute("INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
            [("@UserId", userId), ("@RoleId", roleId)]);
        return Task.CompletedTask;
    }

    public Task RemoveUserFromRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
    {
        Execute("DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId",
            [("@UserId", userId), ("@RoleId", roleId)]);
        return Task.CompletedTask;
    }

    private IdentityUser? FindUser(string predicate, string value)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, $"SELECT * FROM AspNetUsers WHERE {predicate}", [("@Value", value)]);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadUser(reader) : null;
    }

    private IdentityRole? FindRole(string predicate, string value)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, $"SELECT * FROM AspNetRoles WHERE {predicate}", [("@Value", value)]);
        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new IdentityRole
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Name = ReadNullableString(reader, "Name"),
                NormalizedName = ReadNullableString(reader, "NormalizedName"),
                ConcurrencyStamp = ReadNullableString(reader, "ConcurrencyStamp")
            }
            : null;
    }

    private int Execute(string sql, IEnumerable<(string Name, object? Value)> parameters)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, sql, parameters);
        return command.ExecuteNonQuery();
    }

    private static IDbCommand CreateCommand(IDbConnection connection, string sql, IEnumerable<(string Name, object? Value)> parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        return command;
    }

    private static IEnumerable<(string Name, object? Value)> CreateUserParameters(IdentityUser user)
    {
        return
        [
            ("@Id", user.Id), ("@UserName", user.UserName), ("@NormalizedUserName", user.NormalizedUserName),
            ("@Email", user.Email), ("@NormalizedEmail", user.NormalizedEmail), ("@EmailConfirmed", user.EmailConfirmed),
            ("@PasswordHash", user.PasswordHash), ("@SecurityStamp", user.SecurityStamp), ("@ConcurrencyStamp", user.ConcurrencyStamp),
            ("@PhoneNumber", user.PhoneNumber), ("@PhoneNumberConfirmed", user.PhoneNumberConfirmed), ("@TwoFactorEnabled", user.TwoFactorEnabled),
            ("@LockoutEnd", user.LockoutEnd), ("@LockoutEnabled", user.LockoutEnabled), ("@AccessFailedCount", user.AccessFailedCount)
        ];
    }

    private static IdentityUser ReadUser(IDataReader reader)
    {
        return new IdentityUser
        {
            Id = reader.GetString(reader.GetOrdinal("Id")),
            UserName = ReadNullableString(reader, "UserName"),
            NormalizedUserName = ReadNullableString(reader, "NormalizedUserName"),
            Email = ReadNullableString(reader, "Email"),
            NormalizedEmail = ReadNullableString(reader, "NormalizedEmail"),
            EmailConfirmed = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed")),
            PasswordHash = ReadNullableString(reader, "PasswordHash"),
            SecurityStamp = ReadNullableString(reader, "SecurityStamp"),
            ConcurrencyStamp = ReadNullableString(reader, "ConcurrencyStamp"),
            PhoneNumber = ReadNullableString(reader, "PhoneNumber"),
            PhoneNumberConfirmed = reader.GetBoolean(reader.GetOrdinal("PhoneNumberConfirmed")),
            TwoFactorEnabled = reader.GetBoolean(reader.GetOrdinal("TwoFactorEnabled")),
            LockoutEnd = reader.IsDBNull(reader.GetOrdinal("LockoutEnd"))
                ? null
                : new DateTimeOffset(reader.GetDateTime(reader.GetOrdinal("LockoutEnd"))),
            LockoutEnabled = reader.GetBoolean(reader.GetOrdinal("LockoutEnabled")),
            AccessFailedCount = reader.GetInt32(reader.GetOrdinal("AccessFailedCount"))
        };
    }

    private static string? ReadNullableString(IDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
