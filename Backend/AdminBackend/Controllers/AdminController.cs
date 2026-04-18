using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Data;
using AdminBackend.Models;

namespace AdminBackend.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- API ENDPOINTS ---

        [HttpPost("api/admin/login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
        {
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => 
                a.Email.ToLower() == request.Email.Trim().ToLower() && 
                a.Password == request.Password);

            if (admin != null)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return Json(new { success = true, email = admin.Email });
            }

            return Unauthorized(new { message = "INVALID_CREDENTIALS // AUTH_FAILED" });
        }

        [HttpGet("api/admin/metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var stats = new
            {
                TotalProjects = await _context.Projects.CountAsync(),
                TotalSupervisors = await _context.Supervisors.CountAsync(),
                TotalResearchAreas = await _context.ResearchAreas.CountAsync(),
                RevealedCount = await _context.Projects.CountAsync(p => p.IsRevealed),
                PendingCount = await _context.Projects.CountAsync(p => !p.IsRevealed)
            };

            return Json(stats);
        }

        [HttpGet("api/admin/logout")]
        public IActionResult ApiLogout()
        {
            HttpContext.Session.Clear();
            return Json(new { success = true });
        }

        // --- MVC VIEW ACTIONS (Fallback) ---

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var normalizedEmail = email?.Trim().ToLower();
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail && a.Password == password);

            if (admin != null)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Dashboard");
            }

            ViewBag.ErrorMessage = "INVALID_CREDENTIALS // AUTH_FAILED";
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            ViewBag.TotalProjects = await _context.Projects.CountAsync();
            ViewBag.TotalSupervisors = await _context.Supervisors.CountAsync();
            ViewBag.TotalResearchAreas = await _context.ResearchAreas.CountAsync();
            ViewBag.RevealedCount = await _context.Projects.CountAsync(p => p.IsRevealed);
            ViewBag.PendingCount = await _context.Projects.CountAsync(p => !p.IsRevealed);
            return View();
        }

        public async Task<IActionResult> AllocationOverview()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Supervisors = await _context.Supervisors.ToListAsync();
            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllocation(int projectId, int supervisorId)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                project.SupervisorId = supervisorId;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AllocationOverview");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReveal(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.IsRevealed = !project.IsRevealed;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AllocationOverview");
        }

        public async Task<IActionResult> UserManagement()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            ViewBag.Supervisors = await _context.Supervisors.ToListAsync();
            return View();
        }

        public async Task<IActionResult> Analysis()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            ViewBag.Supervisors = await _context.Supervisors.ToListAsync();

            var researchAreas = await _context.ResearchAreas.ToListAsync();
            var areaCounts = researchAreas.Select(ra => new {
                Name = ra.Name,
                Count = _context.Projects.Count(p => p.ResearchAreaId == ra.Id)
            }).ToList();
            ViewBag.AreaCounts = areaCounts;

            return View();
        }

        public async Task<IActionResult> AdminUsers()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var admins = await _context.AdminUsers.ToListAsync();
            return View(admins);
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string email, string password)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            _context.AdminUsers.Add(new AdminUser
            {
                Email = email,
                Password = password
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminUsers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            if (await _context.AdminUsers.CountAsync() <= 1)
            {
                return RedirectToAction("AdminUsers");
            }

            var admin = await _context.AdminUsers.FindAsync(id);
            if (admin != null)
            {
                _context.AdminUsers.Remove(admin);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminUsers");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
