using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using complaint_MS.Models;

namespace complaint_MS.Controllers
{
    public class HomeController : Controller
    {
        // ── PUBLIC LANDING PAGE ────────────────────────────────
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(Constants.AppRoles.Admin))
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }

        // ── SECURE RESIDENT DASHBOARD ──────────────────────────
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        // ── STANDARD ERROR PAGE ────────────────────────────────
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}