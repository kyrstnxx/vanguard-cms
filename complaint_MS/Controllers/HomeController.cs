using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace complaint_MS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");

            return View();
        }
    }
}
