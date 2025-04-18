using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels.forAdmin
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<string> Roles { get; set; }
        public IEnumerable<UserProjectInfo> Projects { get; set; }
    }

    public class UserProjectInfo
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string MemberRole { get; set; }
    }
}
