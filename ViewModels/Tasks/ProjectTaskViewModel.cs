namespace ProjectManagementSystem.Views.ProjectTasks
{
    public class ProjectTaskViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public bool IsReadOnly { get; set; } = false;
    }
}
