using System.ComponentModel.DataAnnotations;

namespace AdminBackend.Models;

public class ResearchArea
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<Project>? Projects { get; set; }
}
