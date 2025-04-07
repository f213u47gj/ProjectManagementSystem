using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.ViewModels.forProject;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ProjectMembersController : Controller
    {
        private readonly IProjectMemberRepository _repository;

        public ProjectMembersController(IProjectMemberRepository repository)
        {
            _repository = repository;
        }

        private async Task<bool> UserCanManageProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false;

            var ownerId = await _repository.GetProjectOwnerIdAsync(projectId);
            if (!string.IsNullOrEmpty(ownerId) && ownerId == userId)
                return true;

            var member = await _repository.GetProjectMemberAsync(projectId, userId);
            return member?.Role == "Manager";
        }

        public async Task<IActionResult> Index(int projectId)
        {
            var members = await _repository.GetProjectMembersAsync(projectId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentMember = await _repository.GetProjectMemberAsync(projectId, userId);
            var isOwner = await _repository.GetProjectOwnerIdAsync(projectId) == userId;

            ViewBag.CanManage = isOwner || (currentMember?.Role == "Manager");

            var viewModel = new ProjectMembersViewModel
            {
                ProjectId = projectId,
                Members = members.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int projectId, string email)
        {
            if (!await UserCanManageProject(projectId))
                return Forbid();

            var success = await _repository.AddMemberByEmailAsync(projectId, email);
            if (!success)
            {
                TempData["Error"] = "Не удалось добавить участника. Проверьте почту.";
            }

            return RedirectToAction("Index", new { projectId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            if (!await UserCanManageProject(projectId))
                return Forbid();

            await _repository.RemoveMemberAsync(projectId, userId);
            return RedirectToAction("Index", new { projectId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int projectId, string userId, string newRole)
        {
            if (!await UserCanManageProject(projectId))
                return Forbid();

            await _repository.ChangeMemberRoleAsync(projectId, userId, newRole);
            return RedirectToAction("Index", new { projectId });
        }
    }
}