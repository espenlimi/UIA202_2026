using System.Data;
using Dapper;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.DataAccess;

public class DapperResourceRepository(IResourceDbConnectionFactory connectionFactory) : IResourceRepository
{
    private const string SelectColumns = """
        Id,
        Name,
        Description,
        Type,
        COALESCE(Address, '') AS Address,
        COALESCE(City, '') AS City,
        COALESCE(ZipCode, '') AS ZipCode,
        COALESCE(OwnerTelephoneNumber, '') AS OwnerTelephoneNumber
        """;

    public Resource Create(ResourceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        connection.Execute(
            """
            INSERT INTO Resources (Name, Description, Type)
            VALUES (@Name, @Description, @Type)
            """,
            model);

        var id = connection.ExecuteScalar<int>("SELECT LAST_INSERT_ID()");
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
        return connection.QuerySingleOrDefault<Resource>(
            $"SELECT {SelectColumns} FROM Resources WHERE Id = @Id",
            new { Id = id });
    }

    public IReadOnlyCollection<Resource> GetAll()
    {
        using var connection = connectionFactory.CreateConnection();
        return connection.Query<Resource>($"SELECT {SelectColumns} FROM Resources").AsList().AsReadOnly();
    }

    public bool Update(int id, ResourceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var connection = connectionFactory.CreateConnection();
        return connection.Execute(
            """
            UPDATE Resources
            SET Name = @Name, Description = @Description, Type = @Type
            WHERE Id = @Id
            """,
            new { model.Name, model.Description, model.Type, Id = id }) > 0;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        return connection.Execute("DELETE FROM Resources WHERE Id = @Id", new { Id = id }) > 0;
    }
}
