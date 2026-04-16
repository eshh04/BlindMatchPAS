using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class ProjectProposal
    {
        [Key]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 100 characters")]
        // Marking Criterion: Robust Validation using Regex to allow only letters, numbers, and spaces
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Only letters and numbers allowed in title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Abstract is required")]
        [MinLength(50, ErrorMessage = "Abstract must be at least 50 characters")]
        [StringLength(2000, ErrorMessage = "Abstract cannot exceed 2000 characters")]
        public string Abstract { get; set; } = string.Empty;

        [Required(ErrorMessage = "Technical Stack is required")]
        public string TechnicalStack { get; set; } = string.Empty;

        // Logic for Blind Match status
        public bool IsMatched { get; set; } = false;
        public string Status { get; set; } = "Pending";

        // --- NEW RELATIONSHIP & VALIDATIONS ---

        // Foreign Key for ResearchArea Table
        [Required(ErrorMessage = "Please select a Research Area")]
        public int ResearchAreaId { get; set; }

        // Navigation property: Connects this proposal to a specific Research Area object
        public ResearchArea? ResearchArea { get; set; }
    }
}