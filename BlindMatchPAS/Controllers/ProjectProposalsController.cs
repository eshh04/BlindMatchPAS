using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    public class ProjectProposalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectProposalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProjectProposals
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProjectProposals.ToListAsync());
        }

        // GET: ProjectProposals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectProposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(m => m.ProjectID == id);
            if (projectProposal == null)
            {
                return NotFound();
            }

            return View(projectProposal);
        }

        // GET: ProjectProposals/Create
        public IActionResult Create()
        {
            // Fetch research areas from DB to show in the dropdown
            ViewBag.ResearchAreaId = new SelectList(_context.ResearchAreas, "Id", "Name");
            return View();
        }

        // POST: ProjectProposals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectID,Title,Abstract,TechnicalStack,ResearchArea,IsMatched,Status")] ProjectProposal projectProposal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectProposal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(projectProposal);
        }

        // GET: ProjectProposals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectProposal = await _context.ProjectProposals.FindAsync(id);
            if (projectProposal == null)
            {
                return NotFound();
            }
            return View(projectProposal);
        }

        // POST: ProjectProposals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProjectID,Title,Abstract,TechnicalStack,ResearchArea,IsMatched,Status")] ProjectProposal projectProposal)
        {
            if (id != projectProposal.ProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectProposal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectProposalExists(projectProposal.ProjectID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(projectProposal);
        }

        // GET: ProjectProposals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectProposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(m => m.ProjectID == id);
            if (projectProposal == null)
            {
                return NotFound();
            }

            return View(projectProposal);
        }

        // POST: ProjectProposals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectProposal = await _context.ProjectProposals.FindAsync(id);
            if (projectProposal != null)
            {
                _context.ProjectProposals.Remove(projectProposal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectProposalExists(int id)
        {
            return _context.ProjectProposals.Any(e => e.ProjectID == id);
        }
    }
}
