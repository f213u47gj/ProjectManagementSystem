using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels.forProject
{
    public class CreateProjectViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
