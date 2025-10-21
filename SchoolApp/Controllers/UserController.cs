using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Services;
using System.Security.Claims;
using SchoolApp.Dto;

namespace SchoolApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IApplicationService applicationService;
        private readonly ILogger<UserController> logger;

        public UserController(IApplicationService applicationService, ILogger<UserController> logger)
        {
            this.applicationService = applicationService;
            this.logger = logger;   
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole("Teacher"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else if (User.IsInRole("Student"))
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return RedirectToAction("AccessDenied", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ClaimsPrincipal? principal = HttpContext.User;

            if (!principal!.Identity!.IsAuthenticated)
            {
                return View();
            }

            //return RedirectToDashboard(principal);
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto credentials)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                var user = await applicationService.UserService.VerifyAndGetUserAsync(credentials);

                if (user == null)
                {
                    ViewData["ValidateMessage"] = "Bad Credentials. Username or Password is invalid.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Usually the user ID
                    new Claim(ClaimTypes.Name, user.Username), // This sets User.Identity.Name
                    new Claim(ClaimTypes.Role, user.UserRole.ToString()!)
                };

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new()
                {
                    AllowRefresh = true,
                    IsPersistent = credentials.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity), properties);
                
                // Redirect based on role
                //ClaimsPrincipal? principal = HttpContext.User;
                var principal = new ClaimsPrincipal(identity);
                //return RedirectToDashboard(principal);
                logger.LogInformation("User {Username} logged in", principal.Identity?.Name);
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                ViewData["ValidateMessage"] = ex.Message;
                return View();
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            logger.LogInformation("User {UserName} logged out", username);
            return RedirectToAction("Login", "User");
        }
    }
}

//private IActionResult RedirectToDashboard(ClaimsPrincipal user)
//{
//    if (user.IsInRole("Admin"))
//    {
//        return RedirectToAction("Index", "Admin");
//    }
//    else if (user.IsInRole("Teacher"))
//    {
//        return RedirectToAction("Index", "Teacher");
//    }
//    else if (user.IsInRole("Student"))
//    {
//        return RedirectToAction("Index", "Student");
//    }
//    else
//    {
//        return RedirectToAction("Index", "Home"); // Fallback
//    }
//}

