using Heimevernet.Web.Models.ViewModels.Resource;
using Microsoft.AspNetCore.Mvc;

namespace Heimevernet.Web.Controllers
{
    public class ResourceHandlerController : Controller
    {
        public IActionResult Index()
        {
            return View(new ResourceViewModel());
        }

        [HttpPost]
        public ActionResult Create(ResourceViewModel model) 
        { 
            //oppdater database med nye data og returner resultat
            return View(model);
        }
    }
}
