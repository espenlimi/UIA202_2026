using Heimevernet.Web.Controllers;
using Heimevernet.Web.Models.ViewModels.Resource;
using Microsoft.AspNetCore.Mvc;

namespace Heimevernet.Web.UnitTests;

public class ResourceControllerTests
{
    [Fact]
    public void Index_ReturnsViewWithSeededResource()
    {
        var controller = new ResourceController();

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ResourceViewModel>(viewResult.Model);
        Assert.Equal("Traktor", model.Name);
        Assert.Equal("Traktor med henger, parkert på åker og enger", model.Description);
        Assert.Equal("Kjøretøy", model.Type);
    }
}
