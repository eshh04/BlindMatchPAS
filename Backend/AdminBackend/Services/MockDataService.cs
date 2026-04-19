using AdminBackend.Models;
using AdminBackend.Enums;

namespace AdminBackend.Services
{
    public static class MockDataService
    {
        public static List<ResearchArea> ResearchAreas { get; set; } = new List<ResearchArea>
        {
            new ResearchArea { Id = 1, Name = "Artificial Intelligence", Description = "AI and Machine Learning" },
            new ResearchArea { Id = 2, Name = "Web Development", Description = "Modern web technologies" },
            new ResearchArea { Id = 3, Name = "Cybersecurity", Description = "Network security and cryptography" },
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
            new Project { Id = 1, Title = "AI-Powered Code Review System", StudentId = "stu-1", ResearchAreaId = 1, SupervisorId = 1, IsRevealed = false, Abstract = "Developing an AI system to assist in code reviews", Status = ProjectStatus.Pending },
            new Project { Id = 2, Title = "Blockchain Security Analysis", StudentId = "stu-2", ResearchAreaId = 3, SupervisorId = 2, IsRevealed = true, Abstract = "Analyzing security vulnerabilities in blockchain implementations", Status = ProjectStatus.Matched },
            new Project { Id = 3, Title = "React Performance Optimization", StudentId = "stu-3", ResearchAreaId = 2, SupervisorId = 3, IsRevealed = false, Abstract = "Optimizing React applications for better performance", Status = ProjectStatus.UnderReview }
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
                project.Supervisor = Supervisors.FirstOrDefault(s => s.Id == supervisorId);
            }
        }
    }
}
