using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels.forAdmin
{
    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public List<string> Roles { get; set; }

        public List<ProjectWithRole> Projects { get; set; }

        public class ProjectWithRole
        {
            public Project Project { get; set; }
            public string Role { get; set; }
        }
    }
}
