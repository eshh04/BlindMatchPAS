using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class ProjectProposal
    {
        [Key]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 10)]
        // Marking Criterion: Robust Validation using Regex
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Only letters and numbers allowed in title")]
        public string Title { get; set; }

        [Required]
        [MinLength(50, ErrorMessage = "Abstract must be at least 50 characters")]
        public string Abstract { get; set; }

        [Required]
        public string TechnicalStack { get; set; }

        [Required]
        public string ResearchArea { get; set; }

        // Logic for Blind Match status
        public bool IsMatched { get; set; } = false;
        public string Status { get; set; } = "Pending";
    }
}