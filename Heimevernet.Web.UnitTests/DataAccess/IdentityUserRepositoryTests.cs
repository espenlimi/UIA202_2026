using System.Data;
using System.Data.Common;
using Heimevernet.Web.DataAccess;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.DataAccess;

public class IdentityUserRepositoryTests
{
    [Fact]
    public async Task AdoNet_CreateUserAsync_UsesParameterizedIdentityInsert()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var repository = new AdoNetIdentityUserRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        await repository.CreateUserAsync(new IdentityUser("student@example.test"), CancellationToken.None);

        Assert.Contains("INSERT INTO AspNetUsers", command.CommandText);
        Assert.Contains("@PasswordHash", command.CommandText);
    }

    [Fact]
    public async Task Dapper_CreateUserAsync_UsesParameterizedIdentityInsert()
    {
        var command = Substitute.For<DbCommand>();
        command.Parameters.Returns(Substitute.For<DbParameterCollection>());
        command.CreateParameter().Returns(Substitute.For<DbParameter>());
        command.ExecuteNonQueryAsync(Arg.Any<CancellationToken>()).Returns(1);

        var connection = Substitute.For<IDbConnection>();
        connection.State.Returns(ConnectionState.Open);
        connection.CreateCommand().Returns(command);

        var factory = Substitute.For<IResourceDbConnectionFactory>();
        factory.CreateConnection().Returns(connection);
        var repository = new DapperIdentityUserRepository(factory);

        await repository.CreateUserAsync(new IdentityUser("student@example.test"), CancellationToken.None);

        Assert.Contains("INSERT INTO AspNetUsers", command.CommandText);
        Assert.Contains("@PasswordHash", command.CommandText);
    }

    [Fact]
    public async Task AdoNet_UpdateUserAsync_UsesOriginalConcurrencyStamp()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var repository = new AdoNetIdentityUserRepository(SqlRepositoryTestHelpers.CreateFactory(command));
        var user = new IdentityUser("student@example.test");

        var updated = await repository.UpdateUserAsync(
            user,
            "original-concurrency-stamp",
            CancellationToken.None);

        Assert.True(updated);
        Assert.Contains("ConcurrencyStamp = @OriginalConcurrencyStamp", command.CommandText);
    }

    [Fact]
    public async Task Dapper_UpdateUserAsync_UsesOriginalConcurrencyStamp()
    {
        var command = Substitute.For<DbCommand>();
        command.Parameters.Returns(Substitute.For<DbParameterCollection>());
        command.CreateParameter().Returns(Substitute.For<DbParameter>());
        command.ExecuteNonQueryAsync(Arg.Any<CancellationToken>()).Returns(1);

        var connection = Substitute.For<IDbConnection>();
        connection.State.Returns(ConnectionState.Open);
        connection.CreateCommand().Returns(command);

        var factory = Substitute.For<IResourceDbConnectionFactory>();
        factory.CreateConnection().Returns(connection);
        var repository = new DapperIdentityUserRepository(factory);
        var user = new IdentityUser("student@example.test");

        var updated = await repository.UpdateUserAsync(
            user,
            "original-concurrency-stamp",
            CancellationToken.None);

        Assert.True(updated);
        Assert.Contains("ConcurrencyStamp = @OriginalConcurrencyStamp", command.CommandText);
    }

    [Fact]
    public async Task AdoNet_GetUserRoleNamesAsync_ReturnsRoleNames()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add("Admin");
        var command = SqlRepositoryTestHelpers.CreateCommand(reader: table.CreateDataReader());
        var repository = new AdoNetIdentityUserRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var roles = await repository.GetUserRoleNamesAsync("user-id", CancellationToken.None);

        Assert.Equal(["Admin"], roles);
        Assert.Contains("SELECT r.Name", command.CommandText);
    }

    [Fact]
    public async Task Dapper_GetUserRoleNamesAsync_ReturnsRoleNames()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add("Admin");
        var command = Substitute.For<DbCommand>();
        command.Parameters.Returns(Substitute.For<DbParameterCollection>());
        command.CreateParameter().Returns(Substitute.For<DbParameter>());
        command.ExecuteReaderAsync(Arg.Any<CommandBehavior>(), Arg.Any<CancellationToken>())
            .Returns(table.CreateDataReader());

        var connection = Substitute.For<IDbConnection>();
        connection.State.Returns(ConnectionState.Open);
        connection.CreateCommand().Returns(command);

        var factory = Substitute.For<IResourceDbConnectionFactory>();
        factory.CreateConnection().Returns(connection);
        var repository = new DapperIdentityUserRepository(factory);

        var roles = await repository.GetUserRoleNamesAsync("user-id", CancellationToken.None);

        Assert.Equal(["Admin"], roles);
        Assert.Contains("SELECT r.Name", command.CommandText);
    }

    [Fact]
    public async Task AdoNet_IsUserInRoleAsync_QueriesByNormalizedRoleName()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(commandResult: 1);
        var repository = new AdoNetIdentityUserRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var isInRole = await repository.IsUserInRoleAsync("user-id", "ADMIN", CancellationToken.None);

        Assert.True(isInRole);
        Assert.Contains("r.NormalizedName = @NormalizedRoleName", command.CommandText);
    }

    [Fact]
    public async Task Dapper_IsUserInRoleAsync_QueriesByNormalizedRoleName()
    {
        var command = Substitute.For<DbCommand>();
        command.Parameters.Returns(Substitute.For<DbParameterCollection>());
        command.CreateParameter().Returns(Substitute.For<DbParameter>());
        command.ExecuteScalarAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(1));

        var connection = Substitute.For<IDbConnection>();
        connection.State.Returns(ConnectionState.Open);
        connection.CreateCommand().Returns(command);

        var factory = Substitute.For<IResourceDbConnectionFactory>();
        factory.CreateConnection().Returns(connection);
        var repository = new DapperIdentityUserRepository(factory);

        var isInRole = await repository.IsUserInRoleAsync("user-id", "ADMIN", CancellationToken.None);

        Assert.True(isInRole);
        Assert.Contains("r.NormalizedName = @NormalizedRoleName", command.CommandText);
    }

    [Fact]
    public async Task AdoNet_FindUserByIdAsync_InterpretsLockoutDateAsUtc()
    {
        var storedLockoutEnd = new DateTime(2026, 10, 22, 10, 0, 0, DateTimeKind.Unspecified);
        var table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("UserName", typeof(string));
        table.Columns.Add("NormalizedUserName", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("NormalizedEmail", typeof(string));
        table.Columns.Add("EmailConfirmed", typeof(bool));
        table.Columns.Add("PasswordHash", typeof(string));
        table.Columns.Add("SecurityStamp", typeof(string));
        table.Columns.Add("ConcurrencyStamp", typeof(string));
        table.Columns.Add("PhoneNumber", typeof(string));
        table.Columns.Add("PhoneNumberConfirmed", typeof(bool));
        table.Columns.Add("TwoFactorEnabled", typeof(bool));
        table.Columns.Add("LockoutEnd", typeof(DateTime));
        table.Columns.Add("LockoutEnabled", typeof(bool));
        table.Columns.Add("AccessFailedCount", typeof(int));
        table.Rows.Add(
            "user-id",
            "student@example.test",
            "STUDENT@EXAMPLE.TEST",
            "student@example.test",
            "STUDENT@EXAMPLE.TEST",
            true,
            DBNull.Value,
            "security-stamp",
            "concurrency-stamp",
            DBNull.Value,
            false,
            false,
            storedLockoutEnd,
            true,
            0);
        var command = SqlRepositoryTestHelpers.CreateCommand(reader: table.CreateDataReader());
        var repository = new AdoNetIdentityUserRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var user = await repository.FindUserByIdAsync("user-id", CancellationToken.None);

        Assert.NotNull(user?.LockoutEnd);
        Assert.Equal(TimeSpan.Zero, user.LockoutEnd.Value.Offset);
        Assert.Equal(storedLockoutEnd, user.LockoutEnd.Value.UtcDateTime);
    }
}
