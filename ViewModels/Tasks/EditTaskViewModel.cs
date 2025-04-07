using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels.Tasks
{
    public class EditTaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "todo";
        public DateTime? DueDate { get; set; }
    }
}
