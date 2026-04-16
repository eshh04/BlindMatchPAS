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

        // Configuring relationships and delete behaviors using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Defining the relationship between ResearchArea and ProjectProposal
            modelBuilder.Entity<ProjectProposal>()
                .HasOne(p => p.ResearchArea)
                .WithMany(r => r.ProjectProposals)
                .HasForeignKey(p => p.ResearchAreaId)
                // Prevents a Research Area from being deleted if it has linked proposals
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}