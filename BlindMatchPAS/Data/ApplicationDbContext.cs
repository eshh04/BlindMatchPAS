using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectProposal> ProjectProposals { get; set; }

        // Added ResearchAreas table for Member 6's tasks
        public DbSet<ResearchArea> ResearchAreas { get; set; }

        // Added Supervisors table for Member 4 & 5's tasks
        public DbSet<Supervisor> Supervisors { get; set; }
    }
}