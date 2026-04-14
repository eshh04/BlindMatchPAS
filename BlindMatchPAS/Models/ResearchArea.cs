using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class ResearchArea
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Research Area name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation property: One research area can have many project proposals
        public ICollection<ProjectProposal>? ProjectProposals { get; set; }
    }
}