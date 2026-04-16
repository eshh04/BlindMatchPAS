using System.ComponentModel.DataAnnotations;

namespace AdminBackend.Models
{
    public class Supervisor
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Department { get; set; } = string.Empty;

        // Navigation property for Projects
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
