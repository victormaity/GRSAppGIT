using System.Collections.Generic;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class AuthenticationModel
    {
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool RememberMe { get; set; }
        public List<ProjectId> ProjectIds { get; set; }

        public string SelectedProject { get; set; }
    }
    public class ProjectId
    {
        public string ProjectName { get; set; }
        public int Id { get; set; }
    }
}