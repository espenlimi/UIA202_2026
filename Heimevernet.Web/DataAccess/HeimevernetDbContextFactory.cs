using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Heimevernet.Web.DataAccess;

public class HeimevernetDbContextFactory : IDesignTimeDbContextFactory<HeimevernetDbContext>
{
    public HeimevernetDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__heimevernetdb")
            ?? "Server=localhost;Database=heimevernetdb;User=root;Password=;";

        var options = new DbContextOptionsBuilder<HeimevernetDbContext>()
            .UseMySql(connectionString, ServerVersion.Parse("10.11.0-mariadb"))
            .Options;

        return new HeimevernetDbContext(options);
    }
}
