// ============================================================================
// File: Controllers/StudentController.cs (Version 1 — M2: Student Submission)
// Purpose: Handles all student operations — project submission, editing,
//          withdrawal, and dashboard display.
// Note: IMatchingService dependency NOT included (not needed by any method).
//       The final version adds it in Push 6 for consistency.
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

namespace BlindMatchPAS.Controllers;

/// <summary>
/// Student controller — manages project CRUD operations and dashboard.
/// All actions require the "StudentOnly" authorization policy.
/// </summary>
[Authorize(Policy = "StudentOnly")]
public class StudentController : Controller
{
    private readonly IProjectService _projectService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IProjectService projectService,
        UserManager<ApplicationUser> userManager,
        ILogger<StudentController> logger)
    {
        _projectService = projectService;
        _userManager = userManager;
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

        var projects = await _projectService.GetStudentProjectsAsync(user.Id);
        return View(projects);
    }

    /// <summary>
    /// GET: /Student/Create
    /// Displays the project submission form with research area dropdown.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
        return View();
    }

    /// <summary>
    /// POST: /Student/Create
    /// Validates and creates a new project with Pending status.
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

        try
        {
            var project = await _projectService.CreateProjectAsync(
                model.Title,
                model.Abstract,
                model.TechStack,
                model.ResearchAreaId,
                user.Id);

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
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        if (project.StudentId != user.Id)
            return Forbid();

        var model = new EditProjectViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Abstract = project.Abstract,
            TechStack = project.TechStack,
            ResearchAreaId = project.ResearchAreaId
        };

        ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
        return View(model);
    }

    /// <summary>
    /// POST: /Student/Edit/{id}
    /// Validates and updates an existing project.
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

        if (!ModelState.IsValid)
        {
            ViewBag.ResearchAreas = await _projectService.GetResearchAreaSelectListAsync();
            return View(model);
        }

        var (success, error) = await _projectService.EditProjectAsync(
            model.Id,
            model.Title,
            model.Abstract,
            model.TechStack,
            model.ResearchAreaId,
            user.Id);

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
