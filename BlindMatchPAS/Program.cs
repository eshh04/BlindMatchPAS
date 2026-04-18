// ============================================================================
// File: Program.cs
// Purpose: Application entry point and service configuration for the
//          BlindMatch Project Approval System (PAS).
// Architecture: ASP.NET Core 9.0 MVC with Identity, EF Core (SQLite),
//               Dependency Injection, and custom middleware.
// Reference: PUSL2020 Coursework - Full System Configuration
// ============================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Middleware;

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

    // Apply pending EF Core migrations automatically
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

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
    const string adminEmail = "admin@blindmatch.ac.uk";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            Role = "Admin",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@2026!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

// ── 11. ROUTING: Default route redirects to Account/Login ────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ── START APPLICATION ────────────────────────────────────────────────────────
app.Run();