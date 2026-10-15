using System.Data;
using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.DataAccess;

internal static class SqlRepositoryTestHelpers
{
    internal static IResourceDbConnectionFactory CreateFactory(params IDbCommand[] commands)
    {
        var connection = Substitute.For<IDbConnection>();
        connection.State.Returns(ConnectionState.Open);
        var commandQueue = new Queue<IDbCommand>(commands);
        connection.CreateCommand().Returns(_ => commandQueue.Dequeue());

        var factory = Substitute.For<IResourceDbConnectionFactory>();
        factory.CreateConnection().Returns(connection);
        return factory;
    }

    internal static IDbCommand CreateCommand(
        int nonQueryResult = 0,
        object? commandResult = null,
        IDataReader? reader = null)
    {
        var command = Substitute.For<IDbCommand>();
        command.Parameters.Returns(Substitute.For<IDataParameterCollection>());
        command.CreateParameter().Returns(Substitute.For<IDbDataParameter>());
        command.ExecuteNonQuery().Returns(nonQueryResult);
        command.ExecuteScalar().Returns(commandResult);
        if (reader is not null)
        {
            command.ExecuteReader().Returns(reader);
            command.ExecuteReader(Arg.Any<CommandBehavior>()).Returns(reader);
        }

        return command;
    }

    internal static IDataReader CreateResourceReader(int id, ResourceViewModel model)
    {
        var table = CreateResourceTable();
        table.Rows.Add(id, model.Name, model.Description, model.Type, "Address", "City", "123456", "555-0100");
        return table.CreateDataReader();
    }

    internal static IDataReader CreateEmptyResourceReader()
    {
        return CreateResourceTable().CreateDataReader();
    }

    internal static IDataReader CreateResourceReaderWithNullOptionalFields(ResourceViewModel model)
    {
        var table = CreateResourceTable();
        table.Rows.Add(
            12,
            model.Name,
            model.Description,
            model.Type,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value);
        return table.CreateDataReader();
    }

    internal static void AssertResource(Resource? resource, ResourceViewModel model)
    {
        Assert.NotNull(resource);
        Assert.Equal(12, resource.Id);
        Assert.Equal(model.Name, resource.Name);
        Assert.Equal(model.Description, resource.Description);
        Assert.Equal(model.Type, resource.Type);
        Assert.Equal("Address", resource.Address);
        Assert.Equal("City", resource.City);
        Assert.Equal("123456", resource.ZipCode);
        Assert.Equal("555-0100", resource.OwnerTelephoneNumber);
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
}
