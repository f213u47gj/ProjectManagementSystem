using ProjectManagementSystem.Models;
using System.Collections.Generic;

namespace ProjectManagementSystem.ViewModels.Tasks
{
    public class BoardViewModel
    {
        public Project Project { get; set; }

        public List<ProjectTask> Tasks { get; set; }

        public List<ProjectMember>? Members { get; set; }
        public string CurrentUserRole { get; set; }
        public User CurrentUser { get; set; }
    }
}
