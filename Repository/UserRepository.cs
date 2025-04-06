using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.ViewModels.forUser;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ProjectManagementSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              RoleManager<IdentityRole> roleManager,
                              ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<User> GetCurrentUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> LoginUserAsync(string usernameOrEmail, string password, bool rememberMe)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                ?? await _userManager.FindByEmailAsync(usernameOrEmail);

            if (user == null) return false;

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, rememberMe, false);
            return result.Succeeded;
        }

        public async Task<bool> RegisterUserAsync(RegistrationViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password)) return false;

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null) return false;

            var user = new User
            {
                Email = model.Email,
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return false;

            await AssignUserRole(user);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private async Task AssignUserRole(User user)
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            var role = user.Email == "nikitanik10305@gmail.com" ? "Admin" : "User";
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<bool> UpdateUserProfileAsync(ClaimsPrincipal principal, ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null) return false;

            user.UserName = model.UserName;
            user.AvatarUrl = model.AvatarUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return false;

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            return true;
        }

        public async Task RefreshSignInAsync(User user)
        {
            await _signInManager.SignOutAsync();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim("AvatarUrl", user.AvatarUrl ?? "")
    };

            var identity = new ClaimsIdentity(claims, "Identity.Application");
            var principal = new ClaimsPrincipal(identity);

            await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal);
        }

        public async Task<(bool Success, IEnumerable<IdentityError>? Errors)> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string oldPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return (false, null);

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return (result.Succeeded, result.Errors);
        }

        public async Task<bool> ChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null) return false;

            user.Email = newEmail;
            user.NormalizedEmail = newEmail.ToUpper();

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
