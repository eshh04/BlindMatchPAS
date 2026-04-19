// ============================================================================
// File: Services/ProjectService.cs
// Purpose: Implements IProjectService — CRUD operations for student projects
//          including ownership verification and status lifecycle management.
// Pattern: Service Layer with EF Core data access (scoped lifetime).
// Reference: PUSL2020 Coursework - Section 4.1 (Student Project Management)
// ============================================================================

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Enums;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services;

/// <summary>
/// Concrete implementation of <see cref="IProjectService"/>.
/// All methods verify ownership before mutating project data.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Project> CreateProjectAsync(string title, string abstract_, string techStack, int researchAreaId, string studentId, bool isGroupProject = false, string? groupMemberEmails = null)
    {
        var config = await _context.SubmissionConfigs.FirstOrDefaultAsync();
        bool isLate = config != null && DateTime.UtcNow > config.EndDate;

        var project = new Project
        {
            StudentId = studentId,
            Title = title,
            Abstract = abstract_,
            TechStack = techStack,
            ResearchAreaId = researchAreaId,
            Status = ProjectStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            IsLate = isLate,
            IsGroupProject = isGroupProject,
            GroupMemberEmails = isGroupProject ? groupMemberEmails : null
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Error)> EditProjectAsync(int projectId, string title, string abstract_, string techStack, int researchAreaId, string studentId, bool isGroupProject = false, string? groupMemberEmails = null)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
            return (false, "Project not found");

        // Ownership check — prevents horizontal privilege escalation (allow group members)
        var student = await _context.Users.FindAsync(studentId);
        bool isOwner = project.StudentId == studentId;
        bool isGroupMember = project.IsGroupProject && student != null && !string.IsNullOrEmpty(project.GroupMemberEmails) && project.GroupMemberEmails.Contains(student.Email!);
        
        if (!isOwner && !isGroupMember)
            return (false, "You do not have permission to edit this project");

        // Status lifecycle guards — Matched and Withdrawn projects are immutable
        if (project.Status == ProjectStatus.Matched)
            return (false, "Cannot edit a matched project");

        if (project.Status == ProjectStatus.Withdrawn)
            return (false, "Cannot edit a withdrawn project");

        project.Title = title;
        project.Abstract = abstract_;
        project.TechStack = techStack;
        project.ResearchAreaId = researchAreaId;
        project.UpdatedAt = DateTime.UtcNow;
        project.IsGroupProject = isGroupProject;
        project.GroupMemberEmails = isGroupProject ? groupMemberEmails : null;

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Error)> WithdrawProjectAsync(int projectId, string studentId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
            return (false, "Project not found");

        // Ownership check
        var student = await _context.Users.FindAsync(studentId);
        bool isOwner = project.StudentId == studentId;
        bool isGroupMember = project.IsGroupProject && student != null && !string.IsNullOrEmpty(project.GroupMemberEmails) && project.GroupMemberEmails.Contains(student.Email!);
        
        if (!isOwner && !isGroupMember)
            return (false, "You do not have permission to withdraw this project");

        // Lifecycle guards
        if (project.Status == ProjectStatus.Matched)
            return (false, "Cannot withdraw a matched project");

        if (project.Status == ProjectStatus.Withdrawn)
            return (false, "Project is already withdrawn");

        project.Status = ProjectStatus.Withdrawn;
        project.UpdatedAt = DateTime.UtcNow;

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetStudentProjectsAsync(string studentId, string studentEmail)
    {
        return await _context.Projects
            .Where(p => p.StudentId == studentId || (p.IsGroupProject && p.GroupMemberEmails != null && p.GroupMemberEmails.Contains(studentEmail)))
            .Include(p => p.ResearchArea)
            .OrderByDescending(p => p.SubmittedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectByIdAsync(int projectId)
    {
        return await _context.Projects
            .Include(p => p.ResearchArea)
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetAvailableProjectsAsync(int? researchAreaId = null)
    {
        // Exclude matched and withdrawn projects; exclude those with confirmed matches
        var query = _context.Projects
            .Where(p => p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.UnderReview)
            .Where(p => !_context.Matches.Any(m => m.ProjectId == p.Id && m.Status == MatchStatus.Confirmed))
            .Include(p => p.ResearchArea)
            .AsQueryable();

        if (researchAreaId.HasValue)
            query = query.Where(p => p.ResearchAreaId == researchAreaId.Value);

        // Oldest first — important for Gale-Shapley tie-breaking
        return await query
            .OrderBy(p => p.SubmittedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SelectListItem>> GetResearchAreaSelectListAsync()
    {
        return await _context.ResearchAreas
            .AsNoTracking()
            .Select(ra => new SelectListItem
            {
                Value = ra.Id.ToString(),
                Text = ra.Name
            })
            .ToListAsync();
    }
}
