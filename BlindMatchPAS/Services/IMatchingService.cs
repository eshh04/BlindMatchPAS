// ============================================================================
// File: Services/IMatchingService.cs
// Purpose: Interface defining the contract for matching operations including
//          interest expression, confirmation, and the Gale-Shapley algorithm.
// Pattern: Service Layer Interface for Dependency Injection (DI).
// Reference: PUSL2020 Coursework - Section 4.2/4.3 (Matching & Algorithm)
// ============================================================================

using BlindMatchPAS.DTOs;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services;

/// <summary>
/// Service interface for managing project-to-supervisor matching.
/// Includes express interest, confirm match, run Gale-Shapley, and reveal operations.
/// </summary>
public interface IMatchingService
{
    /// <summary>Record a supervisor's interest in a project. Returns (success, error).</summary>
    Task<(bool Success, string Error)> ExpressInterestAsync(string supervisorId, int projectId);

    /// <summary>Confirm a match, revealing identities and rejecting competitors. Returns (success, error).</summary>
    Task<(bool Success, string Error)> ConfirmMatchAsync(int matchId, string supervisorId);

    /// <summary>Execute the Gale-Shapley Deferred Acceptance algorithm. Returns list of new matches.</summary>
    Task<List<(int ProjectId, string SupervisorId)>> RunStableMatchingAsync();

    /// <summary>Check if a project is available for matching (no confirmed match).</summary>
    Task<bool> IsProjectAvailableAsync(int projectId);

    /// <summary>Get revealed match details for a student (supervisor info). Returns null if not available.</summary>
    Task<RevealedMatchDto?> GetRevealedMatchForStudentAsync(int projectId, string studentId);

    /// <summary>Get revealed match details for a supervisor (student info). Returns null if not available.</summary>
    Task<RevealedMatchDto?> GetRevealedMatchForSupervisorAsync(int matchId, string supervisorId);
}
