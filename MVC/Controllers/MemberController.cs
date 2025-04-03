using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
