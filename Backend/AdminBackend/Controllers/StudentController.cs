using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminBackend.Data;
using AdminBackend.Models;

namespace AdminBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            return Ok(await _context.Students.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return Ok(student);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var existing = await _context.Students.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = student.Name;
            existing.Email = student.Email;
            existing.Password = student.Password;
            existing.Department = student.Department;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // Unlink projects
                var projects = await _context.Projects.Where(p => p.StudentId == id.ToString()).ToListAsync();
                foreach (var p in projects)
                {
                    _context.Projects.Remove(p); // If student is gone, project is usually gone in this PAS
                }
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
