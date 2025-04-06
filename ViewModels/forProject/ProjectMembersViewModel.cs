using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels.forProject
{
    public class ProjectMembersViewModel
    {
        public int ProjectId { get; set; }
        public List<ProjectMember> Members { get; set; } = new();
    }
}
