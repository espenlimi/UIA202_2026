using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.UnitTests.DataAccess;

public class AdoNetResourceRepositoryTests
{
    private static readonly ResourceViewModel Model = new()
    {
        Name = "Resource",
        Description = "Description",
        Type = "Type"
    };

    [Fact]
    public void AdoNet_Create_InsertsAndReturnsResource()
    {
        var insertCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var idCommand = SqlRepositoryTestHelpers.CreateCommand(commandResult: 12);
        var repository = new AdoNetResourceRepository(SqlRepositoryTestHelpers.CreateFactory(insertCommand, idCommand));

        var result = repository.Create(Model);

        Assert.Equal(12, result.Id);
        Assert.Equal(Model.Name, result.Name);
        Assert.Equal(Model.Description, result.Description);
        Assert.Equal(Model.Type, result.Type);
        Assert.Contains("INSERT INTO Resources", insertCommand.CommandText);
        Assert.Contains("LAST_INSERT_ID()", idCommand.CommandText);
    }

    [Fact]
    public void AdoNet_GetById_MapsResource()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateResourceReader(12, Model));
        var repository = new AdoNetResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetById(12);

        SqlRepositoryTestHelpers.AssertResource(result, Model);
        Assert.Contains("WHERE Id = @Id", command.CommandText);
    }

    [Fact]
    public void AdoNet_GetById_WhenMissing_ReturnsNull()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateEmptyResourceReader());
        var repository = new AdoNetResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetById(12);

        Assert.Null(result);
    }

    [Fact]
    public void AdoNet_GetById_MapsNullOptionalFieldsToEmptyStrings()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateResourceReaderWithNullOptionalFields(Model));
        var repository = new AdoNetResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetById(12);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Address);
        Assert.Equal(string.Empty, result.City);
        Assert.Equal(string.Empty, result.ZipCode);
        Assert.Equal(string.Empty, result.OwnerTelephoneNumber);
    }

    [Fact]
    public void AdoNet_GetAll_ReturnsResources()
    {
        var command = SqlRepositoryTestHelpers.CreateCommand(
            reader: SqlRepositoryTestHelpers.CreateResourceReader(12, Model));
        var repository = new AdoNetResourceRepository(SqlRepositoryTestHelpers.CreateFactory(command));

        var result = repository.GetAll();

        Assert.Single(result);
        SqlRepositoryTestHelpers.AssertResource(result.Single(), Model);
        Assert.Contains("FROM Resources", command.CommandText);
    }

    [Fact]
    public void AdoNet_Update_ReturnsWhetherAResourceWasUpdated()
    {
        var existingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var missingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 0);
        var repository = new AdoNetResourceRepository(
            SqlRepositoryTestHelpers.CreateFactory(existingCommand, missingCommand));

        Assert.True(repository.Update(12, Model));
        Assert.False(repository.Update(404, Model));
        Assert.Contains("UPDATE Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }

    [Fact]
    public void AdoNet_Delete_ReturnsWhetherAResourceWasDeleted()
    {
        var existingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 1);
        var missingCommand = SqlRepositoryTestHelpers.CreateCommand(nonQueryResult: 0);
        var repository = new AdoNetResourceRepository(
            SqlRepositoryTestHelpers.CreateFactory(existingCommand, missingCommand));

        Assert.True(repository.Delete(12));
        Assert.False(repository.Delete(404));
        Assert.Contains("DELETE FROM Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }
}
