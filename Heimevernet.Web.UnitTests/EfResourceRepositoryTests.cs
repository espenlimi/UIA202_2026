using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;
using Microsoft.EntityFrameworkCore;

namespace Heimevernet.Web.UnitTests;

public class EfResourceRepositoryTests
{
    [Fact]
    public void Create_PersistsAndReturnsResource()
    {
        using var dbContext = CreateDbContext();
        var repository = new EfResourceRepository(dbContext);
        var model = CreateResourceModel();

        var result = repository.Create(model);

        Assert.NotEqual(0, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
        Assert.Equal(model.Type, result.Type);

        var persisted = dbContext.Resources.Single();
        Assert.Equal(result.Id, persisted.Id);
        Assert.Equal(result.Name, persisted.Name);
    }

    [Fact]
    public void GetById_WhenResourceExists_ReturnsResource()
    {
        using var dbContext = CreateDbContext();
        var resource = AddResource(dbContext);
        var repository = new EfResourceRepository(dbContext);

        var result = repository.GetById(resource.Id);

        Assert.NotNull(result);
        Assert.Equal(resource.Id, result.Id);
        Assert.Equal(resource.Name, result.Name);
    }

    [Fact]
    public void GetById_WhenResourceDoesNotExist_ReturnsNull()
    {
        using var dbContext = CreateDbContext();
        var repository = new EfResourceRepository(dbContext);

        var result = repository.GetById(404);

        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ReturnsAllResources()
    {
        using var dbContext = CreateDbContext();
        AddResource(dbContext, "Resource 1");
        AddResource(dbContext, "Resource 2");
        var repository = new EfResourceRepository(dbContext);

        var result = repository.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, resource => resource.Name == "Resource 1");
        Assert.Contains(result, resource => resource.Name == "Resource 2");
    }

    [Fact]
    public void Update_WhenResourceExists_PersistsChangesAndReturnsTrue()
    {
        using var dbContext = CreateDbContext();
        var resource = AddResource(dbContext);
        var repository = new EfResourceRepository(dbContext);
        var model = CreateResourceModel();

        var result = repository.Update(resource.Id, model);

        Assert.True(result);
        var updated = dbContext.Resources.Find(resource.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated.Name);
        Assert.Equal(model.Description, updated.Description);
        Assert.Equal(model.Type, updated.Type);
    }

    [Fact]
    public void Update_WhenResourceDoesNotExist_ReturnsFalse()
    {
        using var dbContext = CreateDbContext();
        var repository = new EfResourceRepository(dbContext);

        var result = repository.Update(404, CreateResourceModel());

        Assert.False(result);
    }

    [Fact]
    public void Delete_WhenResourceExists_RemovesResourceAndReturnsTrue()
    {
        using var dbContext = CreateDbContext();
        var resource = AddResource(dbContext);
        var repository = new EfResourceRepository(dbContext);

        var result = repository.Delete(resource.Id);

        Assert.True(result);
        Assert.Null(dbContext.Resources.Find(resource.Id));
    }

    [Fact]
    public void Delete_WhenResourceDoesNotExist_ReturnsFalse()
    {
        using var dbContext = CreateDbContext();
        var repository = new EfResourceRepository(dbContext);

        var result = repository.Delete(404);

        Assert.False(result);
    }

    private static HeimevernetDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HeimevernetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) //Why new guid? To ensure a fresh database for each test
            .Options;

        return new HeimevernetDbContext(options);
    }

    private static Resource AddResource(HeimevernetDbContext dbContext, string name = "Existing resource")
    {
        var resource = new Resource
        {
            Name = name,
            Description = "Existing description",
            Type = "Existing type"
        };

        dbContext.Resources.Add(resource);
        dbContext.SaveChanges();
        return resource;
    }

    private static ResourceViewModel CreateResourceModel()
    {
        return new ResourceViewModel
        {
            Name = "Updated resource",
            Description = "Updated description",
            Type = "Updated type"
        };
    }
}
