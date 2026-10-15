using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.UnitTests.DataAccess;

public class DapperResourceRepositoryTests
{
    private static readonly ResourceViewModel Model = new()
    {
        Name = "Resource",
        Description = "Description",
        Type = "Type"
    };

    [Fact]
    public void Dapper_Create_InsertsAndReturnsResource()
    {
        var insertCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var idCommand = SqlRepositoryTestHelpers.CreateCommand(commandResult: 12);
        var factory = SqlRepositoryTestHelpers.CreateFactory(insertCommand, idCommand);
        var repository = new DapperResourceRepository(factory);

        var result = repository.Create(Model);

        Assert.Equal(12, result.Id);
        Assert.Equal(Model.Name, result.Name);
        Assert.Equal(Model.Description, result.Description);
        Assert.Equal(Model.Type, result.Type);
        Assert.Contains("INSERT INTO Resources", insertCommand.CommandText);
        Assert.Contains("LAST_INSERT_ID()", idCommand.CommandText);
    }

    [Fact]
    public void Dapper_GetById_MapsResource()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateResourceReader(12, Model));
        var repository = new DapperResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetById(12);

        SqlRepositoryTestHelpers.AssertResource(result, Model);
        Assert.Contains("WHERE Id = @Id", command.CommandText);
    }

    [Fact]
    public void Dapper_GetById_UsesEmptyStringsForNullOptionalFields()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateResourceReader(12, Model));
        var repository = new DapperResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        repository.GetById(12);

        Assert.Contains("COALESCE(Address, '') AS Address", command.CommandText);
        Assert.Contains("COALESCE(City, '') AS City", command.CommandText);
        Assert.Contains("COALESCE(ZipCode, '') AS ZipCode", command.CommandText);
        Assert.Contains("COALESCE(OwnerTelephoneNumber, '') AS OwnerTelephoneNumber", command.CommandText);
    }

    [Fact]
    public void Dapper_GetById_WhenMissing_ReturnsNull()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateEmptyResourceReader());
        var repository = new DapperResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetById(12);

        Assert.Null(result);
    }

    [Fact]
    public void Dapper_GetAll_ReturnsResources()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateResourceReader(12, Model));
        var repository = new DapperResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetAll();

        Assert.Single(result);
        SqlRepositoryTestHelpers.AssertResource(result.Single(), Model);
        Assert.Contains("FROM Resources", command.CommandText);
    }

    [Fact]
    public void Dapper_Update_ReturnsWhetherAResourceWasUpdated()
    {
        var existingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var missingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 0);
        var factory = SqlRepositoryTestHelpers.CreateFactory(existingCommand, missingCommand);
        var repository = new DapperResourceRepository(factory);

        Assert.True(repository.Update(12, Model));
        Assert.False(repository.Update(404, Model));
        Assert.Contains("UPDATE Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }

    [Fact]
    public void Dapper_Delete_ReturnsWhetherAResourceWasDeleted()
    {
        var existingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var missingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 0);
        var factory = SqlRepositoryTestHelpers.CreateFactory(existingCommand, missingCommand);
        var repository = new DapperResourceRepository(factory);

        Assert.True(repository.Delete(12));
        Assert.False(repository.Delete(404));
        Assert.Contains("DELETE FROM Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }
}
