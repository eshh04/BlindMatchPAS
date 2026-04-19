// ============================================================================
// File: ViewModels/SupervisorBrowseProjectsViewModel.cs
// Purpose: View model for the supervisor project browsing page.
// Reference: PUSL2020 Coursework - Section 4.2 (Browse & Express Interest)
// ============================================================================

using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Supervisor/Projects view.
/// Contains available projects, current preferences, and existing interest records.
/// </summary>
public class SupervisorBrowseProjectsViewModel
{
    /// <summary>Available projects matching the supervisor's research area preferences.</summary>
    public List<Project> Projects { get; set; } = new();

    /// <summary>Supervisor's ranked research area preferences.</summary>
    public List<SupervisorPreference> Preferences { get; set; } = new();

    /// <summary>List of ProjectIds the supervisor has already expressed interest in.</summary>
    public List<int> SupervisorInterests { get; set; } = new();
}
