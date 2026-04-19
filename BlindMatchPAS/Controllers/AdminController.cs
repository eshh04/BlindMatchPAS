using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("/api/admin/metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var totalProjects = await _context.Projects.CountAsync();
            var totalSupervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            var totalResearchAreas = await _context.ResearchAreas.CountAsync();
            var revealedCount = await _context.Projects.CountAsync(p => p.IsRevealed);
            var pendingCount = await _context.Projects.CountAsync(p => p.Status == BlindMatchPAS.Enums.ProjectStatus.Pending);

            return Json(new
            {
                totalProjects,
                totalSupervisors,
                totalResearchAreas,
                revealedCount,
                pendingCount
            });
        }

        // --- MVC VIEW ACTIONS ---

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalProjects = await _context.Projects.AsNoTracking().CountAsync();
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.TotalSupervisors = supervisors.Count;
            ViewBag.TotalResearchAreas = await _context.ResearchAreas.AsNoTracking().CountAsync();
            ViewBag.RevealedCount = await _context.Projects.AsNoTracking().CountAsync(p => p.IsRevealed);
            ViewBag.PendingCount = await _context.Projects.AsNoTracking().CountAsync(p => !p.IsRevealed);
            return View();
        }

        public async Task<IActionResult> AllocationOverview()
        {
            var projects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Student)
                .Include(p => p.ResearchArea)
                .Include(p => p.Matches)
                    .ThenInclude(m => m.Supervisor)
                .ToListAsync();

            ViewBag.Supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.ResearchArea)
                .Include(p => p.Matches)
                    .ThenInclude(m => m.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return RedirectToAction("AllProjects");
            }

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> AddManualAllocation(string studentEmail, string supervisorId, string projectTitle)
        {
            var student = await _userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                // Create student if system fails as backup
                student = new ApplicationUser { UserName = studentEmail, Email = studentEmail, FullName = "Manual Registration", Role = "Student" };
                await _userManager.CreateAsync(student, "Student@123!");
                await _userManager.AddToRoleAsync(student, "Student");
            }

            var project = new Project
            {
                Title = projectTitle,
                Abstract = "Manual allocation created by administrator.",
                TechStack = "N/A",
                StudentId = student.Id,
                ResearchAreaId = 1, // Default or select from UI
                Status = BlindMatchPAS.Enums.ProjectStatus.Matched
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var match = new Match
            {
                ProjectId = project.Id,
                SupervisorId = supervisorId,
                Status = BlindMatchPAS.Enums.MatchStatus.Confirmed,
                IsRevealed = true
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllocationOverview");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllocation(int projectId, string supervisorId)
        {
            // Remove existing matches for this project to re-assign
            var existingMatches = _context.Matches.Where(m => m.ProjectId == projectId);
            _context.Matches.RemoveRange(existingMatches);

            // Create new match
            var newMatch = new Match
            {
                ProjectId = projectId,
                SupervisorId = supervisorId,
                Status = BlindMatchPAS.Enums.MatchStatus.Confirmed,
                IsRevealed = true,
                CreatedAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow
            };

            _context.Matches.Add(newMatch);
            
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                project.Status = BlindMatchPAS.Enums.ProjectStatus.Matched;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("AllocationOverview");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReveal(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                project.IsRevealed = !project.IsRevealed;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AllocationOverview");
        }

        public async Task<IActionResult> UserManagement()
        {
            ViewBag.Supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.Students = await _userManager.GetUsersInRoleAsync("Student");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUserManual(string email, string fullName, string role, string password)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Role = role
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction("UserManagement");
        }

        public async Task<IActionResult> Projects(string status)
        {
            var query = _context.Projects
                .AsNoTracking()
                .Include(p => p.Student)
                .Include(p => p.ResearchArea)
                .Include(p => p.Matches)
                    .ThenInclude(m => m.Supervisor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<BlindMatchPAS.Enums.ProjectStatus>(status, out var projStatus))
                {
                    query = query.Where(p => p.Status == projStatus);
                }
            }

            var projects = await query.ToListAsync();
            return View("AllProjects", projects);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProjectStatus(int projectId, BlindMatchPAS.Enums.ProjectStatus status)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                project.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Projects");
        }

        public async Task<IActionResult> SubmissionSettings()
        {
            var config = await _context.SubmissionConfigs.FirstOrDefaultAsync() 
                         ?? new SubmissionConfig();
            
            if (config.AllowedProjectTypes == "Both")
            {
                config.AllowedProjectTypes = "Individual";
            }
            
            return View(config);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubmissionSettings(SubmissionConfig model)
        {
            var config = await _context.SubmissionConfigs.FirstOrDefaultAsync();
            if (config == null)
            {
                _context.SubmissionConfigs.Add(model);
            }
            else
            {
                config.StartDate = model.StartDate;
                config.EndDate = model.EndDate;
                config.IsActive = model.IsActive;
                config.Title = model.Title;
                config.AllowedProjectTypes = model.AllowedProjectTypes;
                _context.Update(config);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Analysis()
        {
            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.Supervisors = supervisors;

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
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return View(admins);
        }
    }
}
