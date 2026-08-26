using Heimevernet.Web.Controllers;
using Heimevernet.Web.Models.ViewModels.Resource;
using Microsoft.AspNetCore.Mvc;

namespace Heimevernet.Web.UnitTests;

public class ResourceHandlerControllerTests
{
    [Fact]
    public void Index_ReturnsView()
    {
        var controller = new ResourceHandlerController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Create_ReturnsViewWithSubmittedModel()
    {
        var controller = new ResourceHandlerController();
        var model = new ResourceViewModel
        {
            Name = "Lastebil",
            Description = "Lastebil med tilhenger",
            Type = "Kjøretøy"
        };

        var result = controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }
}
