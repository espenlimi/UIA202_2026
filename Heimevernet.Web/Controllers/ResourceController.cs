using Microsoft.AspNetCore.Mvc;

namespace Heimevernet.Web.Controllers
{
    public class ResourceController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new Models.ViewModels.Resource.ResourceViewModel
            {
                Name = "Traktor",
                Description = "Traktor med henger, parkert på åker og enger",
                Type = "Kjøretøy"
            };

            return View(viewModel);
        }
    }
}
