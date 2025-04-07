using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class RoleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
