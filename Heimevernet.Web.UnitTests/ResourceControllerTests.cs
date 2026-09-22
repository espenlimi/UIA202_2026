using Heimevernet.Web.Controllers;
using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Heimevernet.Web.UnitTests;

public class ResourceControllerTests
{
    [Fact]
    public void Index_WhenResourceExists_ReturnsViewWithResource()
    {
        var resource = new Resource
        {
            Id = 1337,
            Name = "Lastebil",
            Description = "Lastebil med tilhenger",
            Type = "Kjøretøy"
        };
        var repository = Substitute.For<IResourceRepository>();
        repository.GetById(resource.Id).Returns(resource);
        var controller = new ResourceController(repository);

        var result = controller.Index(resource.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ResourceViewModel>(viewResult.Model);
        Assert.Equal(resource.Id, model.Id);
        Assert.Equal(resource.Name, model.Name);
        Assert.Equal(resource.Description, model.Description);
        Assert.Equal(resource.Type, model.Type);
    }

    [Fact]
    public void Index_WhenResourceDoesNotExist_ReturnsEmptyViewModel()
    {
        var repository = Substitute.For<IResourceRepository>();
        repository.GetById(404).Returns((Resource?)null);
        var controller = new ResourceController(repository);

        var result = controller.Index(404);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ResourceViewModel>(viewResult.Model);
        Assert.Null(model.Id);
        Assert.Empty(model.Name);
        Assert.Empty(model.Description);
        Assert.Empty(model.Type);
    }
}
