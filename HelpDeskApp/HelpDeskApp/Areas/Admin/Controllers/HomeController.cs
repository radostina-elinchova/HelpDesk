using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
