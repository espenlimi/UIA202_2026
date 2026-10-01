using System.Data;
using MySqlConnector;

namespace Heimevernet.Web.DataAccess;

public interface IResourceDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class MySqlResourceDbConnectionFactory(IConfiguration configuration) : IResourceDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("heimevernetdb")
        ?? throw new InvalidOperationException(
            "The 'heimevernetdb' connection string was not configured. Run the web app through Aspire.");

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
