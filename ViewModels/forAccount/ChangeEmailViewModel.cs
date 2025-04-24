using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels.forAccount
{
    public class ChangeEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;
    }
}
