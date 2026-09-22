using Heimevernet.Web.Models.Entities;

namespace Heimevernet.Web.DataAccess;

public static class ResourceDbSeeder
{
    public static void Seed(HeimevernetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var seedResources = new[]
        {
            new Resource {Id = 1,  Name = "Resource 1", Description = "Db Description 1", Type = "Type A" },
            new Resource {Id = 2,  Name = "Resource 2", Description = "Db Description 2", Type = "Type B" },
            new Resource {Id = 3,  Name = "Resource 3", Description = "Db Description 3", Type = "Type C" }
        };


        foreach (var seedResource in seedResources)
        {
            var resource = dbContext.Resources.FirstOrDefault(r => r.Id == seedResource.Id);

            if (resource is null)
            {
                resource = new Resource { Id = seedResource.Id };
                dbContext.Resources.Add(resource);
            }

            resource.Name = seedResource.Name;
            resource.Description = seedResource.Description;
            resource.Type = seedResource.Type;
        }

        dbContext.SaveChanges();
    }
}
