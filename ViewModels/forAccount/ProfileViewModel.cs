using ProjectManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels.forAccount
{
    public class ProfileViewModel
    {
        public string? AvatarUrl { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

}
