// ============================================================================
// File: Models/ResearchArea.cs
// Purpose: Entity model representing a research area classification used
//          to categorise projects and supervisor preferences.
// Pattern: Code-First EF Core entity with seed data (see ApplicationDbContext).
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Preferences)
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

/// <summary>
/// Represents a research area category (e.g., "Artificial Intelligence", "Cybersecurity").
/// Seeded via EF Core HasData in ApplicationDbContext. Used by both Projects and SupervisorPreferences.
/// </summary>
public class ResearchArea
{
    /// <summary>Primary key for the research area entity.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Display name of the research area (max 100 characters).</summary>
    [Required(ErrorMessage = "Research Area name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}