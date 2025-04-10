using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels.forProject
{
    public class CreateProjectViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Название проекта не может превышать 50 символов.")]
        public string Name { get; set; } = string.Empty;
        [StringLength(150, ErrorMessage = "Описание проекта не может превышать 150 символов.")]
        public string? Description { get; set; }
    }
}
