using Microsoft.EntityFrameworkCore;
using AdminBackend.Models;

namespace AdminBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed ResearchArea table
            modelBuilder.Entity<ResearchArea>().HasData(
                new ResearchArea { Id = 1, Name = "Artificial Intelligence", Description = "AI and Machine Learning research" },
                new ResearchArea { Id = 2, Name = "Cybersecurity", Description = "Network security and cryptography" },
                new ResearchArea { Id = 3, Name = "Web Development", Description = "Modern web technologies and frameworks" }
            );
        }
    }
}
