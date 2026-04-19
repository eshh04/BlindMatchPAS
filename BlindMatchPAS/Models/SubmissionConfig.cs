using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

public class SubmissionConfig
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "Main Submission Round";

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(14);

    public bool IsActive { get; set; } = true;

    [Required]
    public string AllowedProjectTypes { get; set; } = "Individual"; // "Individual", "Group"
}
