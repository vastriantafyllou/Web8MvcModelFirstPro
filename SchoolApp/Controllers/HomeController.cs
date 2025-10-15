using Microsoft.AspNetCore.Mvc;
using SchoolApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SchoolApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // return View();
            ClaimsPrincipal? principal = HttpContext.User;

            if (!principal!.Identity!.IsAuthenticated)
            {
                return View();
            }
            //return RedirectToAction("Index", "Home");   // Dashboard todo move to dashboard
            //return RedirectToDashboard(principal);
            return RedirectToAction("Index", "User");
        }

        private IActionResult RedirectToDashboard(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (user.IsInRole("Teacher"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else if (user.IsInRole("Student"))
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return RedirectToAction("Index", "Home"); // Fallback
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}