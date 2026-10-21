using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.UnitTests.DataAccess;

public class ResourceRepositoryTests
{
    [Fact]
    public void Create_AssignsAnIdAndStoresTheResource()
    {
        var repository = new ResourceRepository();
        var model = CreateResourceModel();

        var result = repository.Create(model);

        Assert.Equal(4, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
        Assert.Equal(model.Type, result.Type);
        Assert.Same(result, repository.GetById(result.Id));
        Assert.Equal(4, repository.GetAll().Count);
    }

    [Fact]
    public void GetById_WhenResourceDoesNotExist_ReturnsNull()
    {
        var repository = new ResourceRepository();

        var result = repository.GetById(404);

        Assert.Null(result);
    }

    [Fact]
    public void Update_WhenResourceExists_UpdatesAndReturnsTrue()
    {
        var repository = new ResourceRepository();
        var model = CreateResourceModel();

        var result = repository.Update(1, model);

        Assert.True(result);
        var updated = repository.GetById(1);
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated.Name);
        Assert.Equal(model.Description, updated.Description);
        Assert.Equal(model.Type, updated.Type);
    }

    [Fact]
    public void Update_WhenResourceDoesNotExist_ReturnsFalse()
    {
        var repository = new ResourceRepository();

        var result = repository.Update(404, CreateResourceModel());

        Assert.False(result);
    }

    [Fact]
    public void Delete_WhenResourceExists_RemovesAndReturnsTrue()
    {
        var repository = new ResourceRepository();

        var result = repository.Delete(1);

        Assert.True(result);
        Assert.Null(repository.GetById(1));
    }

    [Fact]
    public void Delete_WhenResourceDoesNotExist_ReturnsFalse()
    {
        var repository = new ResourceRepository();

        var result = repository.Delete(404);

        Assert.False(result);
    }

    [Fact]
    public void Create_WhenModelIsNull_ThrowsArgumentNullException()
    {
        var repository = new ResourceRepository();

        Assert.Throws<ArgumentNullException>(() => repository.Create(null!));
    }

    [Fact]
    public void Update_WhenModelIsNull_ThrowsArgumentNullException()
    {
        var repository = new ResourceRepository();

        Assert.Throws<ArgumentNullException>(() => repository.Update(1, null!));
    }

    private static ResourceViewModel CreateResourceModel()
    {
        return new ResourceViewModel
        {
            Name = "Test resource",
            Description = "Test description",
            Type = "Test type"
        };
    }
}
