namespace ProjectManagementSystem.ViewModels.forAdmin
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OwnerName { get; set; }
        public IEnumerable<ProjectMemberInfo> Members { get; set; }
    }

    public class ProjectMemberInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
