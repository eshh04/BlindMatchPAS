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

        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed AdminUser table
            modelBuilder.Entity<AdminUser>().HasData(
                new AdminUser { Id = 1, Email = "admin@pas.com", Password = "123" }
            );

            // Seed Student table
            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, Name = "John Doe", Email = "john@student.com", Password = "123", Department = "CS" },
                new Student { Id = 2, Name = "Jane Roe", Email = "jane@student.com", Password = "123", Department = "IS" },
                new Student { Id = 3, Name = "Sam Smith", Email = "sam@student.com", Password = "123", Department = "SE" },
                new Student { Id = 4, Name = "Emma Watson", Email = "emma@student.com", Password = "123", Department = "CS" },
                new Student { Id = 5, Name = "Bruce Wayne", Email = "bruce@student.com", Password = "123", Department = "IS" }
            );

            // Seed ResearchArea table
            modelBuilder.Entity<ResearchArea>().HasData(
                new ResearchArea { Id = 1, Name = "Artificial Intelligence", Description = "AI and Machine Learning research" },
                new ResearchArea { Id = 2, Name = "Cybersecurity", Description = "Network security and cryptography" },
                new ResearchArea { Id = 3, Name = "Web Development", Description = "Modern web technologies and frameworks" },
                new ResearchArea { Id = 4, Name = "Data Science", Description = "Big data and analytics" },
                new ResearchArea { Id = 5, Name = "Blockchain", Description = "Decentralized ledger technology" }
            );

            // Seed Supervisor table
            modelBuilder.Entity<Supervisor>().HasData(
                new Supervisor { Id = 1, Name = "Dr. Alice Smith", Email = "alice@supervisor.com", Password = "123", Department = "Computer Science" },
                new Supervisor { Id = 2, Name = "Prof. Bob Jones", Email = "bob@supervisor.com", Password = "123", Department = "Information Technology" },
                new Supervisor { Id = 3, Name = "Dr. Charlie Brown", Email = "charlie@supervisor.com", Password = "123", Department = "Software Engineering" },
                new Supervisor { Id = 4, Name = "Dr. Diana Prince", Email = "diana@supervisor.com", Password = "123", Department = "Cyber Studies" },
                new Supervisor { Id = 5, Name = "Prof. Edward Norton", Email = "edward@supervisor.com", Password = "123", Department = "Artificial Intelligence" }
            );

            // Seed Project table
            modelBuilder.Entity<Project>().HasData(
                new Project { Id = 1, Title = "AI for Health", Abstract = "Developing an AI system to assist in code reviews", StudentId = 1, ResearchAreaId = 1, SupervisorId = 1, IsRevealed = false },
                new Project { Id = 2, Title = "Secure Network Protocol", Abstract = "Analyzing security vulnerabilities in blockchain implementations", StudentId = 2, ResearchAreaId = 2, SupervisorId = 2, IsRevealed = true },
                new Project { Id = 3, Title = "Cloud Scalability", Abstract = "Optimizing React applications for better performance", StudentId = 3, ResearchAreaId = 3, SupervisorId = 3, IsRevealed = false },
                new Project { Id = 4, Title = "Privacy in ML", Abstract = "Researching privacy preserving ML techniques", StudentId = 4, ResearchAreaId = 1, SupervisorId = 4, IsRevealed = false },
                new Project { Id = 5, Title = "Threat Detection", Abstract = "Network threat detection using heuristics", StudentId = 5, ResearchAreaId = 2, SupervisorId = 5, IsRevealed = true }
            );
        }
    }
}
