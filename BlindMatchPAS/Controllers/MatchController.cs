// ============================================================================
// File: Controllers/MatchController.cs
// Purpose: Handles match-related operations — running the Gale-Shapley stable
//          matching algorithm (Admin), viewing match status, and revealing
//          matched identities (Student/Supervisor).
// Pattern: MVC Controller with service layer (IMatchingService) and Identity.
// Reference: PUSL2020 Coursework - Section 4.3 (Matching Algorithm) &
//            Section 4.1/4.2 (Identity Reveal)
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Enums;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Controllers;

/// <summary>
/// Match controller — coordinates the matching algorithm, status display, and identity reveal.
/// Contains role-specific actions for Admin, Student, and Supervisor.
/// </summary>
[Authorize]
public class MatchController : Controller
{
    private readonly IMatchingService _matchingService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MatchController> _logger;

    public MatchController(
        IMatchingService matchingService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<MatchController> logger)
    {
        _matchingService = matchingService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET: /Match/RunMatching (Admin only)
    /// Executes the Gale-Shapley Deferred Acceptance stable matching algorithm.
    /// Matches available projects to supervisors based on preference rankings.
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RunMatching()
    {
        try
        {
            var newMatches = await _matchingService.RunStableMatchingAsync();

            _logger.LogInformation("Stable matching completed. {Count} projects matched.", newMatches.Count);

            ViewBag.MatchedCount = newMatches.Count;
            ViewBag.NewMatches = newMatches;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running stable matching algorithm");
            return View("Error", new { message = "Error running matching algorithm: " + ex.Message });
        }
    }

    /// <summary>
    /// GET: /Match/Reveal/{projectId} (Student only)
    /// Reveals the matched supervisor's identity to the student.
    /// Only accessible after the project has been matched and confirmed.
    /// </summary>
    [HttpGet]
    [Route("Match/Reveal/{projectId}")]
    [Authorize] // Allow all authenticated users (logic inside restricts by role/ownership)
    public async Task<IActionResult> Reveal(int projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var isSupervisor = await _userManager.IsInRoleAsync(user, "Supervisor");

        var project = await _context.Projects
            .Include(p => p.Student)
            .Include(p => p.Matches)
                .ThenInclude(m => m.Supervisor)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null || project.Status != ProjectStatus.Matched)
            return RedirectToAction("AccessDenied", "Account", new { error = "not_found_or_unauthorized" });

        var confirmedMatch = project.Matches?.FirstOrDefault(m => m.Status == MatchStatus.Confirmed && m.IsRevealed);
        
        if (confirmedMatch == null || confirmedMatch.Supervisor == null)
            return RedirectToAction("AccessDenied", "Account", new { error = "not_found_or_unauthorized" });

        // Authorization Check
        if (!isAdmin && project.StudentId != user.Id && confirmedMatch.SupervisorId != user.Id)
        {
            return RedirectToAction("AccessDenied", "Account", new { error = "not_found_or_unauthorized" });
        }

        var revealedMatch = new BlindMatchPAS.DTOs.RevealedMatchDto
        {
            MatchId = confirmedMatch.Id,
            PersonFullName = confirmedMatch.Supervisor.FullName,
            PersonEmail = confirmedMatch.Supervisor.Email!,
            ExtraInfo = $"Research Area", // Simplified since ResearchArea might not be included
            MatchedAt = confirmedMatch.ConfirmedAt ?? confirmedMatch.CreatedAt,
            IsGroupProject = project.IsGroupProject,
            GroupMemberEmails = project.GroupMemberEmails
        };

        return View(revealedMatch);
    }

    /// <summary>
    /// GET: /Match/SupervisorReveal/{matchId} (Supervisor only)
    /// Reveals the matched student's identity to the supervisor.
    /// Only accessible after match confirmation.
    /// </summary>
    [HttpGet]
    [Route("Match/SupervisorReveal/{matchId?}")]
    [Authorize] // Allow all authenticated users (logic inside restricts by role/ownership)
    public async Task<IActionResult> SupervisorReveal(int? matchId)
    {
        // Support both route parameter and query string
        int actualMatchId = matchId ?? (int.TryParse(Request.Query["matchId"], out var qId) ? qId : 0);

        if (actualMatchId == 0) return RedirectToAction("AccessDenied", "Account", new { error = "invalid_id" });

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var isStudent = await _userManager.IsInRoleAsync(user, "Student");

        // Use custom query for Admin/Student bypass since the matching service restricts to Supervisor owner
        var match = await _context.Matches
            .Include(m => m.Project)
                .ThenInclude(p => p!.Student)
            .FirstOrDefaultAsync(m => m.Id == actualMatchId);

        if (match == null || match.Status != MatchStatus.Confirmed || !match.IsRevealed || match.Project?.Student == null)
            return RedirectToAction("AccessDenied", "Account", new { error = "not_found_or_unauthorized" });

        // Authorization Check
        if (!isAdmin && match.SupervisorId != user.Id && match.Project.StudentId != user.Id)
        {
            return RedirectToAction("AccessDenied", "Account", new { error = "not_found_or_unauthorized" });
        }

        var revealedMatch = new BlindMatchPAS.DTOs.RevealedMatchDto
        {
            MatchId = match.Id,
            PersonFullName = match.Project.Student.FullName,
            PersonEmail = match.Project.Student.Email!,
            ExtraInfo = $"Project: {match.Project.Title}",
            MatchedAt = match.ConfirmedAt ?? match.CreatedAt,
            IsGroupProject = match.Project.IsGroupProject,
            GroupMemberEmails = match.Project.GroupMemberEmails
        };

        return View(revealedMatch);
    }

    /// <summary>
    /// GET: /Match/Status/{projectId} (Student only)
    /// Displays the current match status for a student's project,
    /// including a visual status pipeline (Submitted → Under Review → Matched).
    /// </summary>
    [Authorize(Policy = "StudentOnly")]
    public async Task<IActionResult> Status(int projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var project = await _context.Projects
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound();

        // Security: Verify ownership
        if (project.StudentId != user.Id)
            return Forbid();

        // Check for a confirmed match on this project
        var match = await _context.Matches
            .Include(m => m.Supervisor)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.Status == MatchStatus.Confirmed);

        var viewModel = new MatchStatusViewModel
        {
            Project = project,
            Match = match,
            IsMatched = match != null,
            IsSupervisorRevealed = match?.IsRevealed ?? false
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: /Match/SupervisorStatus/{matchId} (Supervisor only)
    /// Displays the match status from the supervisor's perspective,
    /// including confirmation state and student identity reveal.
    /// </summary>
    [Authorize(Policy = "SupervisorOnly")]
    public async Task<IActionResult> SupervisorStatus(int matchId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var match = await _context.Matches
            .Include(m => m.Project)
                .ThenInclude(p => p!.Student)
            .Include(m => m.Supervisor)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null || match.SupervisorId != user.Id)
            return NotFound();

        var viewModel = new SupervisorMatchStatusViewModel
        {
            Match = match,
            IsConfirmed = match.Status == MatchStatus.Confirmed,
            IsRevealed = match.IsRevealed,
            StudentName = match.Project?.Student?.FullName ?? "Unknown"
        };

        return View(viewModel);
    }
}
