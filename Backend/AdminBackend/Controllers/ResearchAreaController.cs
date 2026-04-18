using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Data;
using AdminBackend.Models;

namespace AdminBackend.Controllers
{
    public class ResearchAreaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResearchAreaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- API ENDPOINTS ---

        [HttpGet("api/researchareas")]
        public async Task<IActionResult> GetAll()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            return Json(await _context.ResearchAreas.ToListAsync());
        }

        [HttpPost("api/researchareas")]
        public async Task<IActionResult> ApiCreate([FromBody] ResearchArea area)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            _context.ResearchAreas.Add(area);
            await _context.SaveChangesAsync();
            return Json(area);
        }

        [HttpDelete("api/researchareas/{id}")]
        public async Task<IActionResult> ApiDelete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null)
            {
                _context.ResearchAreas.Remove(area);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // --- MVC VIEW ACTIONS (Fallback) ---

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            var areas = await _context.ResearchAreas.ToListAsync();
            return View(areas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            _context.ResearchAreas.Add(new ResearchArea
            {
                Name = name,
                Description = description
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null) return RedirectToAction("Index");

            return View(area);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string description)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null)
            {
                area.Name = name;
                area.Description = description;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Admin");

            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null)
            {
                _context.ResearchAreas.Remove(area);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
