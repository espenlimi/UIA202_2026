using Heimevernet.Web.Controllers;
using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Heimevernet.Web.UnitTests.Controllers;

public class ResourceHandlerControllerTests
{
    [Fact]
    public void Index_ReturnsViewWithResources()
    {
        var resource = GetResource();
        var repository = Substitute.For<IResourceRepository>();
        repository.GetAll().Returns(new[] { resource });
        var controller = CreateController(repository);

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<ResourceViewModel>>(viewResult.Model);
        var returnedResource = Assert.Single(model);
        Assert.Equal(resource.Id, returnedResource.Id);
        Assert.Equal(resource.Name, returnedResource.Name);
        Assert.Equal(resource.Description, returnedResource.Description);
        Assert.Equal(resource.Type, returnedResource.Type);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var controller = CreateController();

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Create_Post_RedirectsToResource()
    {
        //Arrange (please do not comment your own code like this.. )
        var model = GetResourceModel(); //Get a ViewModel object to submit to the controller
        var repository = Substitute.For<IResourceRepository>(); //Initialize the mock
        var expectedNewId = 1337; //The ID we want the repository to return when a new resource is created


        //Now, mock the create method
        repository.Create(Arg.Any<ResourceViewModel>()).Returns(callInfo =>
        {
            //CallInfo gives whatever parameters that were passed to the method.
            var submittedModel = callInfo.Arg<ResourceViewModel>();
            return new Resource
            {
                Id = expectedNewId,
                Name = submittedModel.Name,
                Description = submittedModel.Description,
                Type = submittedModel.Type
            };
        });
        var controller = CreateController(repository);


        //Act
        var result = controller.Create(model);


        //Assert (but first, fetch some assertable results)
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Resource", redirectResult.ControllerName);
        Assert.Equal(expectedNewId, redirectResult.RouteValues!["id"]);

        //Verify number of calls to the create method with the specified parameters
        repository.Received(1).Create(model);
    }

    [Fact]
    public void Create_Post_WhenModelIsInvalid_ReturnsViewWithSubmittedModel()
    {
        var model = GetResourceModel();
        var repository = Substitute.For<IResourceRepository>();
        var controller = CreateController(repository);
        controller.ModelState.AddModelError(nameof(model.Name), "Name is too long");

        var result = controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        repository.DidNotReceive().Create(Arg.Any<ResourceViewModel>());
    }

    [Fact]
    public void Edit_Get_WhenResourceExists_ReturnsViewWithResource()
    {
        var resource = GetResource();
        var repository = Substitute.For<IResourceRepository>();
        repository.GetById(resource.Id).Returns(resource);
        var controller = CreateController(repository);

        var result = controller.Edit(resource.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ResourceViewModel>(viewResult.Model);
        Assert.Equal(resource.Id, model.Id);
        Assert.Equal(resource.Name, model.Name);
        Assert.Equal(resource.Description, model.Description);
        Assert.Equal(resource.Type, model.Type);
    }

    [Fact]
    public void Edit_Get_WhenResourceDoesNotExist_ReturnsNotFound()
    {
        var repository = Substitute.For<IResourceRepository>();
        repository.GetById(404).Returns((Resource?)null);
        var controller = CreateController(repository);

        var result = controller.Edit(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Edit_Post_WhenModelIsInvalid_ReturnsViewWithSubmittedModel()
    {
        var model = GetResourceModel(1337);
        var controller = CreateController();
        controller.ModelState.AddModelError(nameof(model.Name), "Name is required");

        var result = controller.Edit(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public void Edit_Post_WhenModelHasNoId_ReturnsBadRequest()
    {
        var repository = Substitute.For<IResourceRepository>();
        var controller = CreateController(repository);

        var result = controller.Edit(GetResourceModel());

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void Edit_Post_WhenResourceDoesNotExist_ReturnsNotFound()
    {
        var model = GetResourceModel(404);
        var repository = Substitute.For<IResourceRepository>();
        repository.Update(model.Id!.Value, model).Returns(false);
        var controller = CreateController(repository);

        var result = controller.Edit(model);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Edit_Post_WhenResourceIsUpdated_RedirectsToIndex()
    {
        var model = GetResourceModel(1337);
        var repository = Substitute.For<IResourceRepository>();
        repository.Update(model.Id!.Value, model).Returns(true);
        var controller = CreateController(repository);

        var result = controller.Edit(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        repository.Received(1).Update(model.Id.Value, model);
    }

    [Fact]
    public void Delete_WhenResourceExists_RedirectsToIndex()
    {
        var repository = Substitute.For<IResourceRepository>();
        repository.Delete(1337).Returns(true);
        var controller = CreateController(repository);

        var result = controller.Delete(1337);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        repository.Received(1).Delete(1337);
    }

    [Fact]
    public void Delete_WhenResourceDoesNotExist_ReturnsNotFound()
    {
        var repository = Substitute.For<IResourceRepository>();
        repository.Delete(404).Returns(false);
        var controller = CreateController(repository);

        var result = controller.Delete(404);

        Assert.IsType<NotFoundResult>(result);
    }

    private static ResourceHandlerController CreateController(IResourceRepository? repository = null)
    {
        return new ResourceHandlerController(repository ?? Substitute.For<IResourceRepository>());
    }

    private static Resource GetResource()
    {
        return new Resource
        {
            Id = 1337,
            Name = "Lastebil",
            Description = "Lastebil med tilhenger",
            Type = "Kjøretøy"
        };
    }

    private static ResourceViewModel GetResourceModel(int? id = null)
    {
        return new ResourceViewModel
        {
            Id = id,
            Name = "Lastebil",
            Description = "Lastebil med tilhenger",
            Type = "Kjøretøy"
        };
    }
}
