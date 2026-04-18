using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminBackend.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string Abstract { get; set; } = string.Empty;
        
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public int ResearchAreaId { get; set; }
        
        [ForeignKey("ResearchAreaId")]
        public ResearchArea? ResearchArea { get; set; }

        public int SupervisorId { get; set; }
        
        [ForeignKey("SupervisorId")]
        public Supervisor? Supervisor { get; set; }

        public bool IsRevealed { get; set; } = false;
    }
}
