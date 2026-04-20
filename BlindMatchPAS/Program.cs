// ============================================================================
// File: Program.cs
// Purpose: Application entry point and service configuration for the
//          BlindMatch Project Approval System (PAS).
// Architecture: ASP.NET Core 9.0 MVC with Identity, EF Core (SQLite),
//               Dependency Injection, and custom middleware.
// Reference: PUSL2020 Coursework - Full System Configuration
// ============================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Middleware;
using BlindMatchPAS.Enums;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE: EF Core with SQLite ─────────────────────────────────────────
// Configures the ApplicationDbContext to use SQLite as the data store.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. ASP.NET CORE IDENTITY: Authentication & User Management ───────────────
// Configures Identity with password policy, lockout settings, and unique email.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password complexity requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireLowercase = true;

    // Account lockout after 5 failed attempts for 15 minutes
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;

    // Enforce unique email addresses across all users
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── 3. COOKIE AUTHENTICATION: Session Configuration ──────────────────────────
// Redirects unauthenticated users to Login; sessions expire after 8 hours.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── 4. AUTHORIZATION POLICIES: Role-Based Access Control (RBAC) ──────────────
// Three policies correspond to the three user roles in the system.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", p => p.RequireRole("Student"));
    options.AddPolicy("SupervisorOnly", p => p.RequireRole("Supervisor"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ── 5. MVC: Controllers with Views and Anti-Forgery Protection ───────────────
// AutoValidateAntiforgeryToken protects all POST endpoints against CSRF attacks.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// ── 6. DEPENDENCY INJECTION: Application Services ────────────────────────────
// Scoped lifetime ensures one instance per HTTP request (thread-safe for EF Core).
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();

// ── BUILD APPLICATION ────────────────────────────────────────────────────────
var app = builder.Build();

// ── 7. HTTP REQUEST PIPELINE ─────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ── 8. AUTHENTICATION & AUTHORIZATION MIDDLEWARE ─────────────────────────────
// Order matters: Authentication must run before Authorization.
app.UseAuthentication();
app.UseAuthorization();

// ── 9. CUSTOM ROLE-ACCESS MIDDLEWARE ─────────────────────────────────────────
// Prevents students from accessing supervisor routes and vice versa.
app.UseMiddleware<RoleAccessMiddleware>();

// ── 10. SEED DATABASE: Roles, Admin User ─────────────────────────────────────
// Runs on startup to ensure roles exist and a default admin account is created.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    // Try applying migrations first. If legacy SQL Server migrations break on SQLite,
    // recreate the dev database from the current model so the app can run locally.
    try
    {
        dbContext.Database.Migrate();
    }
    catch (SqliteException ex) when (app.Environment.IsDevelopment())
    {
        logger.LogWarning(ex, "SQLite migration failed. Recreating development database from current model.");
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    // Seed the three application roles: Student, Supervisor, Admin
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Student", "Supervisor", "Admin" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Seed default administrator account
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var admin = await userManager.FindByEmailAsync("admin@blindmatch.ac.uk");
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = "admin@blindmatch.ac.uk",
            Email = "admin@blindmatch.ac.uk",
            FullName = "System Administrator",
            Role = "Admin",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin@2026!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
    else if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // ── DEMO SEED DATA: Supervisors, Students & Projects ─────────────────────
    // These accounts are pre-loaded for demonstration and presentation purposes.

    // -- SUPERVISOR ACCOUNTS --
    // All supervisors use password: Supervisor@2026!
    var supervisorSeeds = new[]
    {
        ("Dr. Sarah Mitchell",   "sarah.mitchell@blindmatch.ac.uk",   "Supervisor"),
        ("Dr. James Thornton",   "james.thornton@blindmatch.ac.uk",   "Supervisor"),
        ("Prof. Aisha Rahman",   "aisha.rahman@blindmatch.ac.uk",     "Supervisor"),
    };

    foreach (var (name, email, role) in supervisorSeeds)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var sup = new ApplicationUser
            {
                UserName = email, Email = email,
                FullName = name, Role = role, EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(sup, "Supervisor@2026!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(sup, role);
        }
    }

    // -- STUDENT ACCOUNTS --
    // All students use password: Student@2026!
    var studentSeeds = new[]
    {
        ("Alice Johnson",   "alice.johnson@student.ac.uk"),
        ("Bob Perera",      "bob.perera@student.ac.uk"),
        ("Chaya Fernando",  "chaya.fernando@student.ac.uk"),
    };

    foreach (var (name, email) in studentSeeds)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var stu = new ApplicationUser
            {
                UserName = email, Email = email,
                FullName = name, Role = "Student", EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(stu, "Student@2026!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(stu, "Student");
        }
    }

    // -- DEMO PROJECTS + SUPERVISOR ASSIGNMENTS --
    // Seed only if no projects exist yet in the database.
    if (!dbContext.Projects.Any())
    {
        // Look up research area IDs (seeded by EF Core HasData in DbContext)
        var aiArea   = dbContext.ResearchAreas.FirstOrDefault(r => r.Name.Contains("Artificial") || r.Name.Contains("AI") || r.Name.Contains("Machine"));
        var secArea  = dbContext.ResearchAreas.FirstOrDefault(r => r.Name.Contains("Cyber") || r.Name.Contains("Security"));
        var webArea  = dbContext.ResearchAreas.FirstOrDefault(r => r.Name.Contains("Web") || r.Name.Contains("Software") || r.Name.Contains("Cloud"));
        var fallback = dbContext.ResearchAreas.FirstOrDefault();

        var alice  = await userManager.FindByEmailAsync("alice.johnson@student.ac.uk");
        var bob    = await userManager.FindByEmailAsync("bob.perera@student.ac.uk");
        var chaya  = await userManager.FindByEmailAsync("chaya.fernando@student.ac.uk");
        var sarah  = await userManager.FindByEmailAsync("sarah.mitchell@blindmatch.ac.uk");
        var james  = await userManager.FindByEmailAsync("james.thornton@blindmatch.ac.uk");
        var aisha  = await userManager.FindByEmailAsync("aisha.rahman@blindmatch.ac.uk");

        if (alice != null && bob != null && chaya != null &&
            sarah != null && james != null && aisha != null)
        {
            // Create three matched projects
            var project1 = new Project
            {
                StudentId      = alice.Id,
                Title          = "AI-Powered Student Performance Prediction System",
                Abstract       = "This project aims to develop a machine learning model that predicts student academic performance based on historical data, attendance patterns, and assignment grades. The system will provide early warnings to academic advisors so they can intervene before students fall behind.",
                TechStack      = "Python, TensorFlow, Scikit-learn, Flask, SQLite",
                ResearchAreaId = aiArea?.Id ?? fallback!.Id,
                Status         = ProjectStatus.Matched,
                SubmittedAt    = DateTime.UtcNow.AddDays(-5)
            };
            var project2 = new Project
            {
                StudentId      = bob.Id,
                Title          = "Blockchain-Based Secure Voting System",
                Abstract       = "A decentralised, tamper-proof digital voting platform built on blockchain technology. The system ensures voter anonymity, prevents double voting, and provides a transparent audit trail. Designed for university student union elections.",
                TechStack      = "Solidity, Ethereum, React, Node.js, MetaMask",
                ResearchAreaId = secArea?.Id ?? fallback!.Id,
                Status         = ProjectStatus.Matched,
                SubmittedAt    = DateTime.UtcNow.AddDays(-3)
            };
            var project3 = new Project
            {
                StudentId      = chaya.Id,
                Title          = "Real-Time Collaborative Code Editor with AI Assistance",
                Abstract       = "A web-based IDE that allows multiple developers to collaborate in real time, similar to Google Docs but for code. Integrates an AI assistant powered by a language model to suggest completions, detect bugs, and explain code snippets inline.",
                TechStack      = "ASP.NET Core, SignalR, React, OpenAI API, SQLite",
                ResearchAreaId = webArea?.Id ?? fallback!.Id,
                Status         = ProjectStatus.Matched,
                SubmittedAt    = DateTime.UtcNow.AddDays(-1)
            };

            dbContext.Projects.AddRange(project1, project2, project3);
            await dbContext.SaveChangesAsync();

            // Create confirmed Match records (supervisor assignments) for each project
            dbContext.Matches.AddRange(
                new Match
                {
                    ProjectId    = project1.Id,
                    SupervisorId = sarah.Id,
                    Status       = MatchStatus.Confirmed,
                    IsRevealed   = true,
                    CreatedAt    = DateTime.UtcNow.AddDays(-4),
                    ConfirmedAt  = DateTime.UtcNow.AddDays(-3)
                },
                new Match
                {
                    ProjectId    = project2.Id,
                    SupervisorId = james.Id,
                    Status       = MatchStatus.Confirmed,
                    IsRevealed   = true,
                    CreatedAt    = DateTime.UtcNow.AddDays(-2),
                    ConfirmedAt  = DateTime.UtcNow.AddDays(-1)
                },
                new Match
                {
                    ProjectId    = project3.Id,
                    SupervisorId = aisha.Id,
                    Status       = MatchStatus.Confirmed,
                    IsRevealed   = true,
                    CreatedAt    = DateTime.UtcNow.AddDays(-1),
                    ConfirmedAt  = DateTime.UtcNow
                }
            );
            await dbContext.SaveChangesAsync();
        }
    }

    // --- DATA REPAIR: Ensure all confirmed matches are revealed ---
    var confirmedMatches = await dbContext.Matches
        .Where(m => m.Status == MatchStatus.Confirmed && !m.IsRevealed)
        .ToListAsync();
    
    if (confirmedMatches.Any())
    {
        foreach (var m in confirmedMatches)
        {
            m.IsRevealed = true;
        }
        await dbContext.SaveChangesAsync();
    }
}

// ── 11. ROUTING: Default route redirects to Account/Login ────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ── START APPLICATION ────────────────────────────────────────────────────────
app.Run();