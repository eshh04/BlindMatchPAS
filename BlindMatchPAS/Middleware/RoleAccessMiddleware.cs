// ============================================================================
// File: Middleware/RoleAccessMiddleware.cs
// Purpose: Custom middleware that enforces role-based route access restrictions.
//          Prevents cross-role access (students cannot access supervisor routes
//          and vice versa) at the HTTP pipeline level.
// Pattern: ASP.NET Core custom middleware (RequestDelegate pipeline).
// Reference: PUSL2020 Coursework - Section 3 (RBAC / Access Control)
// ============================================================================

namespace BlindMatchPAS.Middleware;

/// <summary>
/// Middleware to enforce role-based route restrictions.
/// Supplements the [Authorize(Policy)] attributes with URL-level blocking.
/// Registered in Program.cs after UseAuthentication/UseAuthorization.
/// </summary>
public class RoleAccessMiddleware
{
    private readonly RequestDelegate _next;

    public RoleAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Inspects the request path and redirects unauthorised role access to AccessDenied.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            var isStudent = context.User.IsInRole("Student");
            var isSupervisor = context.User.IsInRole("Supervisor");

            // Block students from supervisor routes and match confirmation
            if (isStudent && (path.StartsWith("/supervisor") || path.StartsWith("/match/confirm")))
            {
                context.Response.Redirect("/Account/AccessDenied");
                return;
            }

            // Block supervisors from student routes
            if (isSupervisor && path.StartsWith("/student"))
            {
                context.Response.Redirect("/Account/AccessDenied");
                return;
            }
        }

        await _next(context);
    }
}
