using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin")]
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
            return Json(await _context.ResearchAreas.ToListAsync());
        }

        // --- MVC VIEW ACTIONS ---

        public async Task<IActionResult> Index()
        {
            var areas = await _context.ResearchAreas.ToListAsync();
            return View(areas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            _context.ResearchAreas.Add(new ResearchArea
            {
                Name = name,
                // Description = description // Note: Check ResearchArea model in BlindMatchPAS
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null) return RedirectToAction("Index");

            return View(area);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null)
            {
                area.Name = name;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
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
