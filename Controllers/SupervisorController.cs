// ============================================================================
// File: Controllers/SupervisorController.cs (Version 1 — M4: Supervisor Dashboard)
// Purpose: Handles supervisor operations — browsing projects and managing
//          research area preferences. ExpressInterest and Confirm are NOT
//          yet implemented (added by M5 in Push 6).
// Pattern: MVC Controller with direct DbContext access.
// Security: [Authorize(Policy = "SupervisorOnly")] restricts access to supervisors.
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Functionality)
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Enums;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Controllers;

/// <summary>
/// Supervisor controller — manages project browsing and research area preferences.
/// All actions require the "SupervisorOnly" authorization policy.
/// </summary>
[Authorize(Policy = "SupervisorOnly")]
public class SupervisorController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SupervisorController> _logger;

    public SupervisorController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<SupervisorController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET: /Supervisor/Dashboard
    /// Displays the supervisor's matches grouped by status: Interested, Confirmed, Rejected.
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var matches = await _context.Matches
            .Where(m => m.SupervisorId == user.Id)
            .Include(m => m.Project)
                .ThenInclude(p => p!.ResearchArea)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var viewModel = new SupervisorDashboardViewModel
        {
            InterestedMatches = matches.Where(m => m.Status == MatchStatus.Interested).ToList(),
            ConfirmedMatches = matches.Where(m => m.Status == MatchStatus.Confirmed).ToList(),
            RejectedMatches = matches.Where(m => m.Status == MatchStatus.Rejected).ToList()
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: /Supervisor/Projects
    /// Displays available projects filtered by the supervisor's research area preferences.
    /// </summary>
    public async Task<IActionResult> Projects()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var preferences = await _context.SupervisorPreferences
            .Where(sp => sp.SupervisorId == user.Id)
            .Include(sp => sp.ResearchArea)
            .OrderBy(sp => sp.PreferenceRank)
            .ToListAsync();

        var researchAreaIds = preferences.Select(p => p.ResearchAreaId).ToList();

        List<Project> availableProjects;
        if (researchAreaIds.Any())
        {
            availableProjects = await _context.Projects
                .Where(p => researchAreaIds.Contains(p.ResearchAreaId))
                .Where(p => p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.UnderReview)
                .Where(p => !_context.Matches.Any(m =>
                    m.ProjectId == p.Id && m.Status == MatchStatus.Confirmed))
                .Include(p => p.ResearchArea)
                .OrderBy(p => p.SubmittedAt)
                .ToListAsync();
        }
        else
        {
            availableProjects = new List<Project>();
        }

        var supervisorInterests = await _context.Matches
            .Where(m => m.SupervisorId == user.Id && m.Status == MatchStatus.Interested)
            .Select(m => m.ProjectId)
            .ToListAsync();

        var viewModel = new SupervisorBrowseProjectsViewModel
        {
            Projects = availableProjects,
            Preferences = preferences,
            SupervisorInterests = supervisorInterests
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: /Supervisor/Preferences
    /// Displays the supervisor's ranked research area preferences.
    /// </summary>
    public async Task<IActionResult> Preferences()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var researchAreas = await _context.ResearchAreas
            .AsNoTracking()
            .ToListAsync();

        var existingPreferences = await _context.SupervisorPreferences
            .Where(sp => sp.SupervisorId == user.Id)
            .Include(sp => sp.ResearchArea)
            .OrderBy(sp => sp.PreferenceRank)
            .ToListAsync();

        var viewModel = new SupervisorPreferencesViewModel
        {
            AllResearchAreas = researchAreas,
            Preferences = existingPreferences,
            AvailableAreas = researchAreas
                .Where(ra => !existingPreferences.Any(ep => ep.ResearchAreaId == ra.Id))
                .ToList()
        };

        return View(viewModel);
    }

    /// <summary>
    /// POST: /Supervisor/AddPreference
    /// Adds a research area to the supervisor's preference list.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPreference(int researchAreaId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var existingPref = await _context.SupervisorPreferences
            .FirstOrDefaultAsync(sp => sp.SupervisorId == user.Id && sp.ResearchAreaId == researchAreaId);

        if (existingPref != null)
        {
            TempData["ErrorMessage"] = "You already have this research area in your preferences";
            return RedirectToAction("Preferences");
        }

        var maxRank = await _context.SupervisorPreferences
            .Where(sp => sp.SupervisorId == user.Id)
            .MaxAsync(sp => (int?)sp.PreferenceRank) ?? 0;

        var preference = new SupervisorPreference
        {
            SupervisorId = user.Id,
            ResearchAreaId = researchAreaId,
            PreferenceRank = maxRank + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.SupervisorPreferences.Add(preference);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Supervisor {UserId} added research area {AreaId} to preferences", user.Id, researchAreaId);
        TempData["SuccessMessage"] = "Research area added to your preferences!";

        return RedirectToAction("Preferences");
    }

    /// <summary>
    /// POST: /Supervisor/RemovePreference/{preferenceId}
    /// Removes a research area preference and reorders remaining preferences.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePreference(int preferenceId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var preference = await _context.SupervisorPreferences.FindAsync(preferenceId);
        if (preference == null || preference.SupervisorId != user.Id)
            return Forbid();

        _context.SupervisorPreferences.Remove(preference);

        var remainingPreferences = await _context.SupervisorPreferences
            .Where(sp => sp.SupervisorId == user.Id)
            .OrderBy(sp => sp.PreferenceRank)
            .ToListAsync();

        for (int i = 0; i < remainingPreferences.Count; i++)
            remainingPreferences[i].PreferenceRank = i + 1;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Supervisor {UserId} removed preference {PrefId}", user.Id, preferenceId);
        TempData["SuccessMessage"] = "Research area removed from your preferences";

        return RedirectToAction("Preferences");
    }

    /// <summary>
    /// POST: /Supervisor/ReorderPreferences
    /// Reorders the supervisor's research area preferences.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderPreferences(List<int> preferenceIds)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        if (preferenceIds == null || !preferenceIds.Any())
            return RedirectToAction("Preferences");

        var preferences = await _context.SupervisorPreferences
            .Where(sp => sp.SupervisorId == user.Id && preferenceIds.Contains(sp.Id))
            .ToListAsync();

        for (int i = 0; i < preferenceIds.Count; i++)
        {
            var pref = preferences.FirstOrDefault(p => p.Id == preferenceIds[i]);
            if (pref != null)
                pref.PreferenceRank = i + 1;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Supervisor {UserId} reordered preferences", user.Id);
        TempData["SuccessMessage"] = "Preferences reordered successfully";

        return RedirectToAction("Preferences");
    }
}
