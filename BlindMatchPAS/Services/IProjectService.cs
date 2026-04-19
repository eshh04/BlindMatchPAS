// ============================================================================
// File: Services/IProjectService.cs
// Purpose: Interface defining the contract for project management operations.
// Pattern: Service Layer Interface for Dependency Injection (DI).
// Reference: PUSL2020 Coursework - Section 4.1 (Student Project Management)
// ============================================================================

using Microsoft.AspNetCore.Mvc.Rendering;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services;

/// <summary>
/// Service interface for managing student project submissions and lifecycle.
/// Implemented by ProjectService and registered as scoped in DI container.
/// </summary>
public interface IProjectService
{
    /// <summary>Create a new project with Pending status.</summary>
    Task<Project> CreateProjectAsync(string title, string abstract_, string techStack, int researchAreaId, string studentId);

    /// <summary>Update an existing project. Returns (success, errorMessage).</summary>
    Task<(bool Success, string Error)> EditProjectAsync(int projectId, string title, string abstract_, string techStack, int researchAreaId, string studentId);

    /// <summary>Withdraw a project. Cannot withdraw if already Matched. Returns (success, errorMessage).</summary>
    Task<(bool Success, string Error)> WithdrawProjectAsync(int projectId, string studentId);

    /// <summary>Get all projects for a student, ordered by SubmittedAt descending.</summary>
    Task<List<Project>> GetStudentProjectsAsync(string studentId);

    /// <summary>Get a single project by ID with related data.</summary>
    Task<Project?> GetProjectByIdAsync(int projectId);

    /// <summary>Get available (unmatched) projects, optionally filtered by research area.</summary>
    Task<List<Project>> GetAvailableProjectsAsync(int? researchAreaId = null);

    /// <summary>Get all research areas as SelectListItem for form dropdown binding.</summary>
    Task<IEnumerable<SelectListItem>> GetResearchAreaSelectListAsync();
}
