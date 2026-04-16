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
            // Inclusion of ResearchArea to display the name in the list
            var proposals = await _context.ProjectProposals.Include(p => p.ResearchArea).ToListAsync();
            return View(proposals);
        }

        // GET: ProjectProposals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectProposal = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
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
            // Populate the dropdown with Research Areas from DB
            ViewBag.ResearchAreaId = new SelectList(_context.ResearchAreas, "Id", "Name");
            return View();
        }

        // POST: ProjectProposals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIXED: Changed 'ResearchArea' to 'ResearchAreaId' in the Bind attribute
        public async Task<IActionResult> Create([Bind("ProjectID,Title,Abstract,TechnicalStack,ResearchAreaId,IsMatched,Status")] ProjectProposal projectProposal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectProposal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload the dropdown data before returning to view
            ViewBag.ResearchAreaId = new SelectList(_context.ResearchAreas, "Id", "Name", projectProposal.ResearchAreaId);
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

            ViewBag.ResearchAreaId = new SelectList(_context.ResearchAreas, "Id", "Name", projectProposal.ResearchAreaId);
            return View(projectProposal);
        }

        // POST: ProjectProposals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIXED: Changed 'ResearchArea' to 'ResearchAreaId' in the Bind attribute
        public async Task<IActionResult> Edit(int id, [Bind("ProjectID,Title,Abstract,TechnicalStack,ResearchAreaId,IsMatched,Status")] ProjectProposal projectProposal)
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
            ViewBag.ResearchAreaId = new SelectList(_context.ResearchAreas, "Id", "Name", projectProposal.ResearchAreaId);
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
                .Include(p => p.ResearchArea)
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
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectProposalExists(int id)
        {
            return _context.ProjectProposals.Any(e => e.ProjectID == id);
        }
    }
}