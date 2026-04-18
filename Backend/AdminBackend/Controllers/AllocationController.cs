using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Data;
using AdminBackend.Models;

namespace AdminBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AllocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("matrix")]
        public async Task<IActionResult> GetAllocationMatrix()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var projects = await _context.Projects
                .Include(p => p.Student)
                .ToListAsync();
            var supervisors = await _context.Supervisors.ToListAsync();
            var researchAreas = await _context.ResearchAreas.ToListAsync();
            var students = await _context.Students.ToListAsync();

            return Ok(new
            {
                Projects = projects,
                Supervisors = supervisors,
                ResearchAreas = researchAreas,
                Students = students
            });
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignSupervisor([FromBody] AllocationRequest request)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null) return NotFound("Project not found");

            project.SupervisorId = request.SupervisorId;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Allocation updated" });
        }

        [HttpPost("toggle-reveal/{id}")]
        public async Task<IActionResult> ToggleReveal(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            project.IsRevealed = !project.IsRevealed;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isRevealed = project.IsRevealed });
        }
    }

    public class AllocationRequest
    {
        public int ProjectId { get; set; }
        public int SupervisorId { get; set; }
    }
}
