using System.ComponentModel.DataAnnotations;

namespace AdminBackend.Models
{
    public class ResearchArea
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        // Navigation property for Projects
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
