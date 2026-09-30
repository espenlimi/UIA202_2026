using System.Data;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.DataAccess;

public class AdoNetResourceRepository(IResourceDbConnectionFactory connectionFactory) : IResourceRepository
{
    private const string SelectColumns =
        "Id, Name, Description, Type, Address, City, ZipCode, OwnerTelephoneNumber";

    public Resource Create(ResourceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText =
            "INSERT INTO Resources (Name, Description, Type) VALUES (@Name, @Description, @Type)";
        AddResourceParameters(insertCommand, model);
        insertCommand.ExecuteNonQuery();

        using var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT LAST_INSERT_ID()";
        var id = Convert.ToInt32(idCommand.ExecuteScalar());

        return new Resource
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
            Type = model.Type
        };
    }

    public Resource? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Resources WHERE Id = @Id";
        AddIdParameter(command, id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadResource(reader) : null;
    }

    public IReadOnlyCollection<Resource> GetAll()
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Resources";

        using var reader = command.ExecuteReader();
        var resources = new List<Resource>();
        while (reader.Read())
        {
            resources.Add(ReadResource(reader));
        }

        return resources.AsReadOnly();
    }

    public bool Update(int id, ResourceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE Resources SET Name = @Name, Description = @Description, Type = @Type WHERE Id = @Id";
        AddResourceParameters(command, model);
        AddIdParameter(command, id);
        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Resources WHERE Id = @Id";
        AddIdParameter(command, id);
        return command.ExecuteNonQuery() > 0;
    }

    private static void AddResourceParameters(IDbCommand command, ResourceViewModel model)
    {
        AddParameter(command, "@Name", model.Name);
        AddParameter(command, "@Description", model.Description);
        AddParameter(command, "@Type", model.Type);
    }

    private static void AddIdParameter(IDbCommand command, int id)
    {
        AddParameter(command, "@Id", id);
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static Resource ReadResource(IDataReader reader)
    {
        return new Resource
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = ReadString(reader, "Name"),
            Description = ReadString(reader, "Description"),
            Type = ReadString(reader, "Type"),
            Address = ReadString(reader, "Address"),
            City = ReadString(reader, "City"),
            ZipCode = ReadString(reader, "ZipCode"),
            OwnerTelephoneNumber = ReadString(reader, "OwnerTelephoneNumber")
        };
    }

    private static string ReadString(IDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}
