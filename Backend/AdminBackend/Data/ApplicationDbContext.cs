using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Models;
using AdminBackend.Enums;

namespace AdminBackend.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ResearchArea> ResearchAreas { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<SupervisorPreference> SupervisorPreferences { get; set; }
    public DbSet<Student> Students { get; set; } // Kept for legacy compatibility if needed
    public DbSet<Supervisor> Supervisors { get; set; } // Kept for legacy compatibility if needed
    public DbSet<AdminUser> AdminUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project -> Student (Many-to-One)
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Student)
            .WithMany(u => u.StudentProjects)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Project -> ResearchArea
        modelBuilder.Entity<Project>()
            .HasOne(p => p.ResearchArea)
            .WithMany(r => r.Projects)
            .HasForeignKey(p => p.ResearchAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Project -> Matches
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Matches)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Match -> Supervisor
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Supervisor)
            .WithMany(u => u.SupervisorMatches)
            .HasForeignKey(m => m.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // SupervisorPreference -> Supervisor
        modelBuilder.Entity<SupervisorPreference>()
            .HasOne(sp => sp.Supervisor)
            .WithMany(u => u.ResearchAreaPreferences)
            .HasForeignKey(sp => sp.SupervisorId)
            .OnDelete(DeleteBehavior.Cascade);

        // SupervisorPreference -> ResearchArea
        modelBuilder.Entity<SupervisorPreference>()
            .HasOne(sp => sp.ResearchArea)
            .WithMany()
            .HasForeignKey(sp => sp.ResearchAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupervisorPreference>()
            .HasIndex(sp => new { sp.SupervisorId, sp.ResearchAreaId })
            .IsUnique();

        // Seed Data: Research Areas (Full 10 from main)
        modelBuilder.Entity<ResearchArea>().HasData(
            new ResearchArea { Id = 1, Name = "Artificial Intelligence", Description = "AI and Machine Learning research" },
            new ResearchArea { Id = 2, Name = "Web Development", Description = "Modern web technologies and frameworks" },
            new ResearchArea { Id = 3, Name = "Cybersecurity", Description = "Network security and cryptography" },
            new ResearchArea { Id = 4, Name = "Machine Learning", Description = "Statistical models and algorithms" },
            new ResearchArea { Id = 5, Name = "Cloud Computing", Description = "Distributed systems and cloud infrastructure" },
            new ResearchArea { Id = 6, Name = "Data Science", Description = "Big data analytics and visualization" },
            new ResearchArea { Id = 7, Name = "Internet of Things", Description = "Connected devices and smart systems" },
            new ResearchArea { Id = 8, Name = "Blockchain", Description = "Decentralized ledger technologies" },
            new ResearchArea { Id = 9, Name = "Mobile Development", Description = "iOS and Android app development" },
            new ResearchArea { Id = 10, Name = "Natural Language Processing", Description = "Text and speech processing" }
        );
    }
}
