// ============================================================================
// File: Controllers/StudentController.cs
// Purpose: Handles all student operations — project submission, editing,
//          withdrawal, and dashboard display.
// Pattern: MVC Controller with service layer (IProjectService) and Identity.
// Security: [Authorize(Policy = "StudentOnly")] restricts access to students.
// Reference: PUSL2020 Coursework - Section 4.1 (Student Functionality)
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.ViewModels;
using BlindMatchPAS.Data;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers;

/// <summary>
/// Student controller — manages project CRUD operations and dashboard.
/// All actions require the "StudentOnly" authorization policy.
/// </summary>
[Authorize(Policy = "StudentOnly")]
public class StudentController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IMatchingService _matchingService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IProjectService projectService,
        IMatchingService matchingService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<StudentController> logger)
    {
        _projectService = projectService;
        _matchingService = matchingService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET: /Student/Dashboard
    /// Displays all projects submitted by the currently logged-in student.
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var projects = await _projectService.GetStudentProjectsAsync(user.Id, user.Email!);
        return View(projects);
    }

    /// <summary>
    /// GET: /Student/Create
    /// Displays the project submission form with research area dropdown.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        var config = await _context.SubmissionConfigs.FirstOrDefaultAsync();
        if (config == null || !config.IsActive || DateTime.UtcNow < config.StartDate)
        {
            TempData["ErrorMessage"] = "Project submissions are currently closed.";
            return RedirectToAction("Dashboard");
        }

        ViewBag.SubmissionConfig = config;
        ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
        return View();
    }

    /// <summary>
    /// POST: /Student/Create
    /// Validates and creates a new project with Pending status.
    /// Ownership is automatically assigned to the logged-in student.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var config = await _context.SubmissionConfigs.FirstOrDefaultAsync();
        if (config != null)
        {
            if (config.AllowedProjectTypes == "Individual" && model.IsGroupProject)
            {
                ModelState.AddModelError("IsGroupProject", "Group projects are not permitted in this submission window.");
            }
            else if (config.AllowedProjectTypes == "Group" && !model.IsGroupProject)
            {
                ModelState.AddModelError("IsGroupProject", "Only group projects are permitted in this submission window.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.SubmissionConfig = config;
            ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
            return View(model);
        }

        try
        {
            var project = await _projectService.CreateProjectAsync(
                model.Title,
                model.Abstract,
                model.TechStack,
                model.ResearchAreaId,
                user.Id,
                model.IsGroupProject,
                model.GroupMemberEmails);

            _logger.LogInformation("Student {UserId} created project {ProjectId}", user.Id, project.Id);
            TempData["SuccessMessage"] = "Project submitted successfully!";
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            ModelState.AddModelError(string.Empty, "Error creating project. Please try again.");
            ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
            return View(model);
        }
    }

    /// <summary>
    /// GET: /Student/Edit/{id}
    /// Displays the edit form for an existing project.
    /// Verifies ownership before allowing access.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        // Security: Verify the logged-in student owns this project or is a group member
        bool isOwner = project.StudentId == user.Id;
        bool isGroupMember = project.IsGroupProject && !string.IsNullOrEmpty(project.GroupMemberEmails) && project.GroupMemberEmails.Contains(user.Email!);

        if (!isOwner && !isGroupMember)
            return Forbid();

        var model = new EditProjectViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Abstract = project.Abstract,
            TechStack = project.TechStack,
            ResearchAreaId = project.ResearchAreaId,
            IsGroupProject = project.IsGroupProject,
            GroupMemberEmails = project.GroupMemberEmails
        };

        var config = await _context.SubmissionConfigs.FirstOrDefaultAsync();
        ViewBag.SubmissionConfig = config;
        ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
        return View(model);
    }

    /// <summary>
    /// POST: /Student/Edit/{id}
    /// Validates and updates an existing project.
    /// Cannot edit projects that are Matched or Withdrawn.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditProjectViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        if (id != model.Id)
            return BadRequest();

        var config = await _context.SubmissionConfigs.FirstOrDefaultAsync();
        if (config != null)
        {
            if (config.AllowedProjectTypes == "Individual" && model.IsGroupProject)
            {
                ModelState.AddModelError("IsGroupProject", "Group projects are not permitted in this submission window.");
            }
            else if (config.AllowedProjectTypes == "Group" && !model.IsGroupProject)
            {
                ModelState.AddModelError("IsGroupProject", "Only group projects are permitted in this submission window.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.SubmissionConfig = config;
            ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
            return View(model);
        }

        var (success, error) = await _projectService.EditProjectAsync(
            model.Id,
            model.Title,
            model.Abstract,
            model.TechStack,
            model.ResearchAreaId,
            user.Id,
            model.IsGroupProject,
            model.GroupMemberEmails);

        if (!success)
        {
            _logger.LogWarning("Edit failed: {Error}", error);
            ModelState.AddModelError(string.Empty, error);
            ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
            return View(model);
        }

        _logger.LogInformation("Student {UserId} edited project {ProjectId}", user.Id, id);
        TempData["SuccessMessage"] = "Project updated successfully!";
        return RedirectToAction("Dashboard");
    }

    /// <summary>
    /// POST: /Student/Withdraw/{id}
    /// Withdraws a project. Cannot withdraw if already Matched.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var (success, error) = await _projectService.WithdrawProjectAsync(id, user.Id);

        if (!success)
        {
            _logger.LogWarning("Withdraw failed: {Error}", error);
            TempData["ErrorMessage"] = error;
        }
        else
        {
            _logger.LogInformation("Student {UserId} withdrew project {ProjectId}", user.Id, id);
            TempData["SuccessMessage"] = "Project withdrawn successfully!";
        }

        return RedirectToAction("Dashboard");
    }
}
