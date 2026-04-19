// ============================================================================
// File: Services/MatchingService.cs
// Purpose: Implements IMatchingService — core matching logic including interest
//          expression, match confirmation, the Gale-Shapley Deferred Acceptance
//          stable matching algorithm, and identity reveal for both roles.
// Algorithm: Gale-Shapley (supervisor-proposing variant) with research area
//            preference rankings. Guarantees a stable matching where no
//            unmatched pair mutually prefers each other.
// Pattern: Service Layer with EF Core and explicit transaction management.
// Reference: PUSL2020 Coursework - Section 4.3 (Matching Algorithm)
//            Gale & Shapley (1962) "College Admissions and the Stability of Marriage"
// ============================================================================

using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.DTOs;
using BlindMatchPAS.Enums;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services;

/// <summary>
/// Concrete implementation of <see cref="IMatchingService"/>.
/// Contains the Gale-Shapley stable matching algorithm and match lifecycle methods.
/// </summary>
public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _context;

    public MatchingService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Error)> ExpressInterestAsync(string supervisorId, int projectId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
            return (false, "Project not found");

        // Guard: cannot express interest in withdrawn projects
        if (project.Status == ProjectStatus.Withdrawn)
            return (false, "Cannot express interest in a withdrawn project");

        // Guard: cannot express interest if project already has a confirmed match
        var existingConfirmedMatch = await _context.Matches.FirstOrDefaultAsync(m =>
            m.ProjectId == projectId && m.Status == MatchStatus.Confirmed);
        if (existingConfirmedMatch != null)
            return (false, "This project is already matched");

        // Guard: prevent duplicate interest from the same supervisor
        var existingInterest = await _context.Matches.FirstOrDefaultAsync(m =>
            m.ProjectId == projectId && m.SupervisorId == supervisorId);
        if (existingInterest != null)
            return (false, "You have already expressed interest in this project");

        // Create a new Match record with Interested status
        var match = new Match
        {
            ProjectId = projectId,
            SupervisorId = supervisorId,
            Status = MatchStatus.Interested,
            CreatedAt = DateTime.UtcNow
        };

        _context.Matches.Add(match);

        // Transition project from Pending to UnderReview on first interest
        if (project.Status == ProjectStatus.Pending)
        {
            project.Status = ProjectStatus.UnderReview;
            project.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Error)> ConfirmMatchAsync(int matchId, string supervisorId)
    {
        var match = await _context.Matches
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null)
            return (false, "Match not found");

        // Ownership verification
        if (match.SupervisorId != supervisorId)
            return (false, "You do not have permission to confirm this match");

        // Status guards
        if (match.Status == MatchStatus.Confirmed)
            return (false, "This match is already confirmed");

        if (match.Status != MatchStatus.Interested)
            return (false, "Match is not in Interested state");

        // Use a transaction to atomically confirm match, update project, and reject competitors
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Confirm this match and reveal identities
            match.Status = MatchStatus.Confirmed;
            match.IsRevealed = true;
            match.ConfirmedAt = DateTime.UtcNow;

            // Set project status to Matched
            match.Project!.Status = ProjectStatus.Matched;
            match.Project.UpdatedAt = DateTime.UtcNow;

            // Reject all other interested matches on the same project
            var competitors = await _context.Matches
                .Where(m => m.ProjectId == match.ProjectId &&
                            m.Id != matchId &&
                            m.Status == MatchStatus.Interested)
                .ToListAsync();

            foreach (var competitor in competitors)
            {
                competitor.Status = MatchStatus.Rejected;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error confirming match: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the Gale-Shapley Deferred Acceptance stable matching algorithm.
    /// 
    /// Algorithm overview (supervisor-proposing variant):
    /// 1. Load all supervisors with their ranked research area preferences.
    /// 2. Load all available (unmatched) projects.
    /// 3. Build preference lists: supervisors rank projects by research area preference;
    ///    projects rank supervisors by how highly the supervisor ranked the project's area.
    /// 4. Main loop: each free supervisor proposes to their most-preferred unproposed project.
    ///    - If the project is free, it tentatively accepts.
    ///    - If the project prefers the new proposer over its current holder, it swaps.
    ///    - Otherwise, the proposer is rejected and must propose to the next project.
    /// 5. Loop terminates when all supervisors are matched or have exhausted their lists.
    /// 6. Persist confirmed matches, reject competitors, and update project statuses.
    /// 
    /// Time complexity: O(n * m) where n = supervisors, m = projects.
    /// Stability guarantee: no unmatched pair mutually prefers each other.
    /// </summary>
    public async Task<List<(int ProjectId, string SupervisorId)>> RunStableMatchingAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // STEP 1: Load supervisors, projects, and preferences
        // ═══════════════════════════════════════════════════════════════════════════

        var supervisors = await _context.Users
            .Where(u => u.Role == "Supervisor")
            .Include(u => u.ResearchAreaPreferences)
                .ThenInclude(sp => sp.ResearchArea)
            .ToListAsync();

        var availableProjects = await _context.Projects
            .Include(p => p.ResearchArea)
            .Where(p => p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.UnderReview)
            .Where(p => !_context.Matches.Any(m =>
                m.ProjectId == p.Id && m.Status == MatchStatus.Confirmed))
            .OrderBy(p => p.SubmittedAt)
            .ToListAsync();

        if (!supervisors.Any() || !availableProjects.Any())
            return new List<(int, string)>();

        // ═══════════════════════════════════════════════════════════════════════════
        // STEP 2: Build supervisor preference lists
        // Each supervisor ranks projects by their research area preference ranking.
        // Ties are broken by submission date (oldest first — FIFO fairness).
        // ═══════════════════════════════════════════════════════════════════════════

        var supervisorPreferences = new Dictionary<string, List<int>>();

        foreach (var supervisor in supervisors)
        {
            var rankedAreaIds = supervisor.ResearchAreaPreferences!
                .OrderBy(sp => sp.PreferenceRank)
                .Select(sp => sp.ResearchAreaId)
                .ToList();

            var orderedProjects = availableProjects
                .OrderBy(p =>
                {
                    var rank = rankedAreaIds.IndexOf(p.ResearchAreaId);
                    return rank == -1 ? int.MaxValue : rank;
                })
                .ThenBy(p => p.SubmittedAt)
                .Select(p => p.Id)
                .ToList();

            supervisorPreferences[supervisor.Id] = orderedProjects;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STEP 3: Build project preference lists
        // Each project ranks supervisors by how highly the supervisor ranked the
        // project's research area. Ties broken alphabetically by supervisor name.
        // ═══════════════════════════════════════════════════════════════════════════

        var projectPreferences = new Dictionary<int, List<string>>();

        foreach (var project in availableProjects)
        {
            var orderedSupervisors = supervisors
                .OrderBy(s =>
                {
                    var pref = s.ResearchAreaPreferences!
                        .FirstOrDefault(sp => sp.ResearchAreaId == project.ResearchAreaId);
                    return pref?.PreferenceRank ?? int.MaxValue;
                })
                .ThenBy(s => s.FullName)
                .Select(s => s.Id)
                .ToList();

            projectPreferences[project.Id] = orderedSupervisors;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STEP 4: Gale-Shapley main loop (supervisor-proposing)
        // Each free supervisor proposes to their top unproposed project.
        // Projects accept the best proposer and reject the rest.
        // ═══════════════════════════════════════════════════════════════════════════

        var proposalIndex = supervisors.ToDictionary(s => s.Id, s => 0);
        var tentativeMatch = availableProjects.ToDictionary(p => p.Id, p => (string?)null);
        var freeSupervisors = new Queue<string>(supervisors.Select(s => s.Id));

        while (freeSupervisors.Count > 0)
        {
            var supervisorId = freeSupervisors.Dequeue();
            var preferences = supervisorPreferences[supervisorId];

            // Skip if supervisor has exhausted all proposals
            if (proposalIndex[supervisorId] >= preferences.Count)
                continue;

            // Propose to next preferred project
            var projectId = preferences[proposalIndex[supervisorId]];
            proposalIndex[supervisorId]++;

            var currentHolder = tentativeMatch[projectId];

            if (currentHolder == null)
            {
                // Project is free — tentatively accept the proposer
                tentativeMatch[projectId] = supervisorId;
            }
            else
            {
                // Project already has a tentative match — compare preferences
                var projectRanking = projectPreferences[projectId];
                var newRank = projectRanking.IndexOf(supervisorId);
                var currentRank = projectRanking.IndexOf(currentHolder);

                var newIsBetter = newRank != -1 && (currentRank == -1 || newRank < currentRank);

                if (newIsBetter)
                {
                    // Project prefers new proposer — swap, freeing the old holder
                    tentativeMatch[projectId] = supervisorId;
                    freeSupervisors.Enqueue(currentHolder);
                }
                else
                {
                    // Project prefers current holder — proposer must try next choice
                    freeSupervisors.Enqueue(supervisorId);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STEP 5: Persist confirmed stable matches to the database
        // Create/update Match records, reject competitors, update project statuses.
        // ═══════════════════════════════════════════════════════════════════════════

        var newMatches = new List<(int ProjectId, string SupervisorId)>();

        foreach (var (projectId, supervisorId) in tentativeMatch)
        {
            if (supervisorId == null) continue;

            // Promote existing interest to Confirmed, or create a new Confirmed match
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.SupervisorId == supervisorId);

            if (existingMatch != null)
            {
                existingMatch.Status = MatchStatus.Confirmed;
                existingMatch.IsRevealed = true;
                existingMatch.ConfirmedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Matches.Add(new Match
                {
                    ProjectId = projectId,
                    SupervisorId = supervisorId,
                    Status = MatchStatus.Confirmed,
                    IsRevealed = true,
                    CreatedAt = DateTime.UtcNow,
                    ConfirmedAt = DateTime.UtcNow
                });
            }

            // Reject all other interested matches on this project
            var competitors = await _context.Matches
                .Where(m => m.ProjectId == projectId &&
                            m.SupervisorId != supervisorId &&
                            m.Status == MatchStatus.Interested)
                .ToListAsync();

            foreach (var competitor in competitors)
            {
                competitor.Status = MatchStatus.Rejected;
            }

            // Update project status to Matched
            var project = availableProjects.First(p => p.Id == projectId);
            project.Status = ProjectStatus.Matched;
            project.UpdatedAt = DateTime.UtcNow;

            newMatches.Add((projectId, supervisorId));
        }

        await _context.SaveChangesAsync();
        return newMatches;
    }

    /// <inheritdoc/>
    public async Task<bool> IsProjectAvailableAsync(int projectId)
    {
        var hasConfirmedMatch = await _context.Matches.AnyAsync(m =>
            m.ProjectId == projectId && m.Status == MatchStatus.Confirmed);
        return !hasConfirmedMatch;
    }

    /// <inheritdoc/>
    public async Task<RevealedMatchDto?> GetRevealedMatchForStudentAsync(int projectId, string studentId)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null || project.StudentId != studentId)
            return null;

        // Find the confirmed and revealed match for this project
        var match = await _context.Matches
            .Include(m => m.Supervisor)
                .ThenInclude(s => s!.ResearchAreaPreferences)
                    .ThenInclude(sp => sp.ResearchArea)
            .FirstOrDefaultAsync(m =>
                m.ProjectId == projectId &&
                m.Status == MatchStatus.Confirmed &&
                m.IsRevealed);

        if (match?.Supervisor == null)
            return null;

        // Compile the supervisor's ranked research areas for display
        var areas = string.Join(", ", match.Supervisor.ResearchAreaPreferences!
            .OrderBy(sp => sp.PreferenceRank)
            .Select(sp => sp.ResearchArea.Name));

        return new RevealedMatchDto
        {
            MatchId = match.Id,
            PersonFullName = match.Supervisor.FullName,
            PersonEmail = match.Supervisor.Email!,
            ExtraInfo = areas,
            MatchedAt = match.ConfirmedAt ?? match.CreatedAt
        };
    }

    /// <inheritdoc/>
    public async Task<RevealedMatchDto?> GetRevealedMatchForSupervisorAsync(int matchId, string supervisorId)
    {
        var match = await _context.Matches
            .Include(m => m.Supervisor)
            .Include(m => m.Project)
                .ThenInclude(p => p!.Student)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null || match.SupervisorId != supervisorId)
            return null;

        // Only reveal if confirmed and revealed flag is set
        if (match.Status != MatchStatus.Confirmed || !match.IsRevealed)
            return null;

        if (match.Project?.Student == null)
            return null;

        return new RevealedMatchDto
        {
            MatchId = match.Id,
            PersonFullName = match.Project.Student.FullName,
            PersonEmail = match.Project.Student.Email!,
            ExtraInfo = $"Project: {match.Project.Title}",
            MatchedAt = match.ConfirmedAt ?? match.CreatedAt
        };
    }
}
