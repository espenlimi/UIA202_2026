using System.Data;
using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;
using NSubstitute;

namespace Heimevernet.Web.UnitTests;

public class SqlResourceRepositoryTests
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
        var insertCommand = CreateCommand(nonQueryResult: 1);
        var idCommand = CreateCommand(scalarResult: 12);
        var factory = CreateFactory(insertCommand, idCommand);
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
        var command = CreateCommand(reader: CreateResourceReader(12));
        var repository = new DapperResourceRepository(CreateFactory(command));

        var result = repository.GetById(12);

        AssertResource(result);
        Assert.Contains("WHERE Id = @Id", command.CommandText);
    }

    [Fact]
    public void Dapper_GetById_WhenMissing_ReturnsNull()
    {
        var command = CreateCommand(reader: CreateEmptyResourceReader());
        var repository = new DapperResourceRepository(CreateFactory(command));

        var result = repository.GetById(12);

        Assert.Null(result);
    }

    [Fact]
    public void Dapper_GetAll_ReturnsResources()
    {
        var command = CreateCommand(reader: CreateResourceReader(12));
        var repository = new DapperResourceRepository(CreateFactory(command));

        var result = repository.GetAll();

        Assert.Single(result);
        AssertResource(result.Single());
        Assert.Contains("FROM Resources", command.CommandText);
    }

    [Fact]
    public void Dapper_Update_ReturnsWhetherAResourceWasUpdated()
    {
        var existingCommand = CreateCommand(nonQueryResult: 1);
        var missingCommand = CreateCommand(nonQueryResult: 0);
        var factory = CreateFactory(existingCommand, missingCommand);
        var repository = new DapperResourceRepository(factory);

        Assert.True(repository.Update(12, Model));
        Assert.False(repository.Update(404, Model));
        Assert.Contains("UPDATE Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }

    [Fact]
    public void Dapper_Delete_ReturnsWhetherAResourceWasDeleted()
    {
        var existingCommand = CreateCommand(nonQueryResult: 1);
        var missingCommand = CreateCommand(nonQueryResult: 0);
        var factory = CreateFactory(existingCommand, missingCommand);
        var repository = new DapperResourceRepository(factory);

        Assert.True(repository.Delete(12));
        Assert.False(repository.Delete(404));
        Assert.Contains("DELETE FROM Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }

    [Fact]
    public void AdoNet_Create_InsertsAndReturnsResource()
    {
        var insertCommand = CreateCommand(nonQueryResult: 1);
        var idCommand = CreateCommand(scalarResult: 12);
        var repository = new AdoNetResourceRepository(CreateFactory(insertCommand, idCommand));

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
        var command = CreateCommand(reader: CreateResourceReader(12));
        var repository = new AdoNetResourceRepository(CreateFactory(command));

        var result = repository.GetById(12);

        AssertResource(result);
        Assert.Contains("WHERE Id = @Id", command.CommandText);
    }

    [Fact]
    public void AdoNet_GetById_WhenMissing_ReturnsNull()
    {
        var command = CreateCommand(reader: CreateEmptyResourceReader());
        var repository = new AdoNetResourceRepository(CreateFactory(command));

        var result = repository.GetById(12);

        Assert.Null(result);
    }

    [Fact]
    public void AdoNet_GetById_MapsNullOptionalFieldsToEmptyStrings()
    {
        var command = CreateCommand(reader: CreateResourceReaderWithNullOptionalFields());
        var repository = new AdoNetResourceRepository(CreateFactory(command));

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
        var command = CreateCommand(reader: CreateResourceReader(12));
        var repository = new AdoNetResourceRepository(CreateFactory(command));

        var result = repository.GetAll();

        Assert.Single(result);
        AssertResource(result.Single());
        Assert.Contains("FROM Resources", command.CommandText);
    }

    [Fact]
    public void AdoNet_Update_ReturnsWhetherAResourceWasUpdated()
    {
        var existingCommand = CreateCommand(nonQueryResult: 1);
        var missingCommand = CreateCommand(nonQueryResult: 0);
        var repository = new AdoNetResourceRepository(CreateFactory(existingCommand, missingCommand));

        Assert.True(repository.Update(12, Model));
        Assert.False(repository.Update(404, Model));
        Assert.Contains("UPDATE Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }

    [Fact]
    public void AdoNet_Delete_ReturnsWhetherAResourceWasDeleted()
    {
        var existingCommand = CreateCommand(nonQueryResult: 1);
        var missingCommand = CreateCommand(nonQueryResult: 0);
        var repository = new AdoNetResourceRepository(CreateFactory(existingCommand, missingCommand));

        Assert.True(repository.Delete(12));
        Assert.False(repository.Delete(404));
        Assert.Contains("DELETE FROM Resources", existingCommand.CommandText);
        Assert.Contains("WHERE Id = @Id", existingCommand.CommandText);
    }

    private static IResourceDbConnectionFactory CreateFactory(params IDbCommand[] commands)
    {
        var connection = Substitute.For<IDbConnection>();
        connection.State.Returns(ConnectionState.Open);
        var commandQueue = new Queue<IDbCommand>(commands);
        connection.CreateCommand().Returns(_ => commandQueue.Dequeue());

        var factory = Substitute.For<IResourceDbConnectionFactory>();
        factory.CreateConnection().Returns(connection);
        return factory;
    }

    private static IDbCommand CreateCommand(
        int nonQueryResult = 0,
        object? scalarResult = null,
        IDataReader? reader = null)
    {
        var command = Substitute.For<IDbCommand>();
        command.Parameters.Returns(Substitute.For<IDataParameterCollection>());
        command.CreateParameter().Returns(Substitute.For<IDbDataParameter>());
        command.ExecuteNonQuery().Returns(nonQueryResult);
        command.ExecuteScalar().Returns(scalarResult);
        if (reader is not null)
        {
            command.ExecuteReader().Returns(reader);
            command.ExecuteReader(Arg.Any<CommandBehavior>()).Returns(reader);
        }

        return command;
    }

    private static IDataReader CreateResourceReader(int id)
    {
        var table = CreateResourceTable();
        table.Rows.Add(id, Model.Name, Model.Description, Model.Type, "Address", "City", "123456", "555-0100");
        return table.CreateDataReader();
    }

    private static IDataReader CreateEmptyResourceReader()
    {
        return CreateResourceTable().CreateDataReader();
    }

    private static IDataReader CreateResourceReaderWithNullOptionalFields()
    {
        var table = CreateResourceTable();
        table.Rows.Add(12, Model.Name, Model.Description, Model.Type, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);
        return table.CreateDataReader();
    }

    private static DataTable CreateResourceTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Description", typeof(string));
        table.Columns.Add("Type", typeof(string));
        table.Columns.Add("Address", typeof(string));
        table.Columns.Add("City", typeof(string));
        table.Columns.Add("ZipCode", typeof(string));
        table.Columns.Add("OwnerTelephoneNumber", typeof(string));
        return table;
    }

    private static void AssertResource(Resource? resource)
    {
        Assert.NotNull(resource);
        Assert.Equal(12, resource.Id);
        Assert.Equal(Model.Name, resource.Name);
        Assert.Equal(Model.Description, resource.Description);
        Assert.Equal(Model.Type, resource.Type);
        Assert.Equal("Address", resource.Address);
        Assert.Equal("City", resource.City);
        Assert.Equal("123456", resource.ZipCode);
        Assert.Equal("555-0100", resource.OwnerTelephoneNumber);
    }
}
