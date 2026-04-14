using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class Supervisor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        // Foreign Key for Research Area
        [Required]
        public int ResearchAreaId { get; set; }

        // Navigation property: Each supervisor belongs to one research area
        public ResearchArea? ResearchArea { get; set; }
    }
}