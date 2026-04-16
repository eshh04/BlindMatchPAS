using Microsoft.AspNetCore.Mvc;

namespace AdminBackend.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to overview
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                return RedirectToAction("AllocationOverview");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Hardcoded credentials as requested
            if (email == "admin@pas.com" && password == "password123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("AllocationOverview");
            }

            ViewBag.ErrorMessage = "INVALID_CREDENTIALS // AUTH_FAILED";
            return View();
        }

        public IActionResult AllocationOverview()
        {
            // Basic authentication check
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }

            return Content("ALLOCATION_OVERVIEW // SYSTEM_ACTIVE");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
