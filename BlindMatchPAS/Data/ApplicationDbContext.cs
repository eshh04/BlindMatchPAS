// ============================================================================
// File: Data/ApplicationDbContext.cs
// Purpose: EF Core database context for the BlindMatch application.
//          Configures entity relationships, constraints, and seed data.
// Pattern: IdentityDbContext<T> with Fluent API configuration.
// Reference: PUSL2020 Coursework - Section 5 (Data Layer / EF Core)
// ============================================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Data;

/// <summary>
/// Application database context extending IdentityDbContext for ASP.NET Core Identity.
/// Manages Projects, Matches, SupervisorPreferences, and ResearchAreas entities.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Research area lookup table (seeded with initial data).</summary>
    public DbSet<ResearchArea> ResearchAreas { get; set; }

    /// <summary>Student project proposals.</summary>
    public DbSet<Project> Projects { get; set; }

    /// <summary>Match records linking supervisors to projects.</summary>
    public DbSet<Match> Matches { get; set; }

    /// <summary>Supervisor ranked research area preferences.</summary>
    public DbSet<SupervisorPreference> SupervisorPreferences { get; set; }

    /// <summary>System configuration for project submission rounds.</summary>
    public DbSet<SubmissionConfig> SubmissionConfigs { get; set; }

    /// <summary>
    /// Configures entity relationships and constraints using Fluent API.
    /// Called by EF Core during model creation.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Project → Student (Many-to-One) ──────────────────────────────
        // Each project belongs to one student; a student can have many projects.
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Student)
            .WithMany(u => u.StudentProjects)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Project → ResearchArea (Many-to-One) ─────────────────────────
        // Each project is classified under one research area.
        modelBuilder.Entity<Project>()
            .HasOne(p => p.ResearchArea)
            .WithMany()
            .HasForeignKey(p => p.ResearchAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Project → Matches (One-to-Many) ──────────────────────────────
        // A project can receive interest from multiple supervisors.
        // This is critical for the Gale-Shapley algorithm to work correctly.
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Matches)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Match → Supervisor (Many-to-One) ─────────────────────────────
        // Each match record references one supervisor; a supervisor can have many matches.
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Supervisor)
            .WithMany(u => u.SupervisorMatches)
            .HasForeignKey(m => m.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── SupervisorPreference → Supervisor (Many-to-One) ──────────────
        modelBuilder.Entity<SupervisorPreference>()
            .HasOne(sp => sp.Supervisor)
            .WithMany(u => u.ResearchAreaPreferences)
            .HasForeignKey(sp => sp.SupervisorId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── SupervisorPreference → ResearchArea (Many-to-One) ────────────
        modelBuilder.Entity<SupervisorPreference>()
            .HasOne(sp => sp.ResearchArea)
            .WithMany()
            .HasForeignKey(sp => sp.ResearchAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique Constraint: one preference per supervisor per research area ──
        modelBuilder.Entity<SupervisorPreference>()
            .HasIndex(sp => new { sp.SupervisorId, sp.ResearchAreaId })
            .IsUnique();

        // ── Seed Data: Research Areas ────────────────────────────────────
        // Pre-populated research area categories for the system.
        modelBuilder.Entity<ResearchArea>().HasData(
            new ResearchArea { Id = 1, Name = "Artificial Intelligence" },
            new ResearchArea { Id = 2, Name = "Web Development" },
            new ResearchArea { Id = 3, Name = "Cybersecurity" },
            new ResearchArea { Id = 4, Name = "Machine Learning" },
            new ResearchArea { Id = 5, Name = "Cloud Computing" },
            new ResearchArea { Id = 6, Name = "Data Science" },
            new ResearchArea { Id = 7, Name = "Internet of Things" },
            new ResearchArea { Id = 8, Name = "Blockchain" },
            new ResearchArea { Id = 9, Name = "Mobile Development" },
            new ResearchArea { Id = 10, Name = "Natural Language Processing" }
        );
    }
}