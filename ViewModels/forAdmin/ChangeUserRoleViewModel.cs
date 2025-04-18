using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels.forAdmin
{
    public class ChangeUserRoleViewModel
    {
        public User User { get; set; }
        public string SelectedRole { get; set; }
        public List<SelectListItem> AvailableRoles { get; set; }
    }
}
