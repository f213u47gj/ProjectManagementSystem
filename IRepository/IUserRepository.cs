using Microsoft.AspNetCore.Identity;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using ProjectManagementSystem.ViewModels.forUser;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagementSystem.IRepositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetCurrentUserAsync(string userId);
        Task<User?> GetUserByIdAsync(string userId);
        Task<bool> LoginUserAsync(string usernameOrEmail, string password, bool rememberMe);
        Task<bool> RegisterUserAsync(RegistrationViewModel model);
        Task LogoutAsync();
        Task<bool> UpdateUserProfileAsync(ClaimsPrincipal principal, ProfileViewModel model);
        Task RefreshSignInAsync(User user);
        Task<(bool Success, IEnumerable<IdentityError>? Errors)> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string oldPassword, string newPassword);
        Task<bool> ChangeEmailAsync(ClaimsPrincipal principal, string newEmail);
    }
}
