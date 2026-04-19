using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AdminBackend.Enums;

namespace AdminBackend.Models;

public class Project
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser? Student { get; set; }

    [Required(ErrorMessage = "Project title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Abstract is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Abstract must be between 50 and 2000 characters")]
    public string Abstract { get; set; } = string.Empty;

    [StringLength(500)]
    public string TechStack { get; set; } = string.Empty;

    [Required]
    public int ResearchAreaId { get; set; }

    public ResearchArea? ResearchArea { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsRevealed { get; set; } = false;

    public ICollection<Match>? Matches { get; set; }
    
    // Kept for compatibility with some existing code that might use SupervisorId directly
    public int? SupervisorId { get; set; }
    public Supervisor? Supervisor { get; set; }
}
