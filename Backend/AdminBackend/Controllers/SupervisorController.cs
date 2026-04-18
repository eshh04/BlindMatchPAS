using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Data;
using AdminBackend.Models;

namespace AdminBackend.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- API ENDPOINTS ---

        [HttpGet("api/supervisors")]
        public async Task<IActionResult> GetAll()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            return Json(await _context.Supervisors.ToListAsync());
        }

        [HttpPost("api/supervisors")]
        public async Task<IActionResult> ApiCreate([FromBody] Supervisor supervisor)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            _context.Supervisors.Add(supervisor);
            await _context.SaveChangesAsync();
            return Json(supervisor);
        }

        [HttpDelete("api/supervisors/{id}")]
        public async Task<IActionResult> ApiDelete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var supervisor = await _context.Supervisors.FindAsync(id);
            if (supervisor != null)
            {
                _context.Supervisors.Remove(supervisor);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // --- MVC VIEW ACTIONS (Fallback) ---

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            var supervisors = await _context.Supervisors.ToListAsync();
            return View(supervisors);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string department)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            _context.Supervisors.Add(new Supervisor
            {
                Name = name,
                Department = department
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            var supervisor = await _context.Supervisors.FindAsync(id);
            if (supervisor != null)
            {
                var assignedProjects = await _context.Projects.Where(p => p.SupervisorId == id).ToListAsync();
                foreach(var p in assignedProjects)
                {
                    p.SupervisorId = 0;
                }
                _context.Supervisors.Remove(supervisor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
