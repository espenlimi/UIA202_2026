using Heimevernet.Web.Models;

namespace Heimevernet.Web.UnitTests;

public class ErrorViewModelTests
{
    [Fact]
    public void ShowRequestId_IsFalse_WhenRequestIdIsNullOrEmpty()
    {
        var model = new ErrorViewModel { RequestId = null };
        Assert.False(model.ShowRequestId);

        model.RequestId = "";
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_IsTrue_WhenRequestIdIsSet()
    {
        var model = new ErrorViewModel { RequestId = "abc" };
        Assert.True(model.ShowRequestId);
    }
}
