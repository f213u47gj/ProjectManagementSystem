using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels.forAdmin;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly UserManager<User> _userManager;

        public AdminController(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var viewModels = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var memberProjects = await _projectRepository.GetProjectsByUserIdAsync(user.Id);

                var projectInfos = memberProjects.Select(pm => new UserProjectInfo
                {
                    ProjectId = pm.ProjectId,
                    ProjectName = pm.Project?.Name ?? "Неизвестный проект",
                    MemberRole = pm.Role
                });

                viewModels.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    CreatedAt = user.CreatedAt,
                    Roles = roles,
                    Projects = projectInfos
                });
            }

            return View(viewModels);
        }

        public async Task<IActionResult> Projects()
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
            var viewModels = projects.Select(project => new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                OwnerName = project.Owner?.UserName ?? "Неизвестный владелец",
                Members = project.Members.Select(m => new ProjectMemberInfo
                {
                    UserId = m.UserId,
                    UserName = m.User?.UserName ?? "Неизвестный пользователь",
                    Role = m.Role
                })
            }).ToList();

            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest("Ошибка при удалении пользователя.");

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            await _projectRepository.DeleteProjectAsync(project);
            return RedirectToAction(nameof(Projects));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (!result.Succeeded)
                return BadRequest("Не удалось назначить новую роль.");

            return RedirectToAction(nameof(Users));
        }
    }
}
