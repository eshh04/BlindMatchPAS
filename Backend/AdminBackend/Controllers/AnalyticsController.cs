using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Data;

namespace AdminBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var researchAreas = await _context.ResearchAreas.ToListAsync();
            var projects = await _context.Projects.ToListAsync();

            var areaDistribution = researchAreas.Select(ra => new
            {
                Name = ra.Name,
                Value = projects.Count(p => p.ResearchAreaId == ra.Id)
            }).ToList();

            var supervisorLoad = await _context.Supervisors.Select(s => new
            {
                Name = s.Name,
                ProjectCount = _context.Projects.Count(p => p.SupervisorId == s.Id)
            }).ToListAsync();

            return Ok(new
            {
                AreaDistribution = areaDistribution,
                SupervisorLoad = supervisorLoad,
                TotalSystemLoad = (double)projects.Count(p => p.SupervisorId != 0) / projects.Count * 100
            });
        }
    }
}
