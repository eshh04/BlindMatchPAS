using AdminBackend.Models;

namespace AdminBackend.Services
{
    public static class MockDataService
    {
        public static List<ResearchArea> ResearchAreas { get; set; } = new List<ResearchArea>
        {
            new ResearchArea { Id = 1, Name = "Artificial Intelligence", Description = "AI and Machine Learning" },
            new ResearchArea { Id = 2, Name = "Cybersecurity", Description = "Network security and cryptography" },
            new ResearchArea { Id = 3, Name = "Web Development", Description = "Modern web technologies" },
            new ResearchArea { Id = 4, Name = "Data Science", Description = "Big data and analytics" },
            new ResearchArea { Id = 5, Name = "Blockchain", Description = "Decentralized ledger technology" }
        };

        public static List<Supervisor> Supervisors { get; set; } = new List<Supervisor>
        {
            new Supervisor { Id = 1, Name = "Dr. Alice Smith", Department = "Computer Science" },
            new Supervisor { Id = 2, Name = "Prof. Bob Jones", Department = "Information Technology" },
            new Supervisor { Id = 3, Name = "Dr. Charlie Brown", Department = "Software Engineering" },
            new Supervisor { Id = 4, Name = "Dr. Diana Prince", Department = "Cyber Studies" },
            new Supervisor { Id = 5, Name = "Prof. Edward Norton", Department = "Artificial Intelligence" }
        };

        public static List<Project> Projects { get; set; } = new List<Project>
        {
            new Project { Id = 1, Title = "AI for Health", StudentId = 1, ResearchAreaId = 1, SupervisorId = 1, IsRevealed = false },
            new Project { Id = 2, Title = "Secure Network Protocol", StudentId = 2, ResearchAreaId = 2, SupervisorId = 2, IsRevealed = true },
            new Project { Id = 3, Title = "Cloud Scalability", StudentId = 3, ResearchAreaId = 3, SupervisorId = 3, IsRevealed = false },
            new Project { Id = 4, Title = "Privacy in ML", StudentId = 4, ResearchAreaId = 1, SupervisorId = 4, IsRevealed = false },
            new Project { Id = 5, Title = "Threat Detection", StudentId = 5, ResearchAreaId = 2, SupervisorId = 5, IsRevealed = true }
        };

        public static List<AdminUser> AdminUsers { get; set; } = new List<AdminUser>
        {
            new AdminUser { Id = 1, Email = "admin@pas.com", Password = "123" }
        };

        public static void UpdateAllocation(int projectId, int supervisorId)
        {
            var project = Projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
            {
                project.SupervisorId = supervisorId;
                // Optionally update the navigation property if we want to be thorough in memory
                project.Supervisor = Supervisors.FirstOrDefault(s => s.Id == supervisorId);
            }
        }
    }
}
