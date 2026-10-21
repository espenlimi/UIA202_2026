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
}
