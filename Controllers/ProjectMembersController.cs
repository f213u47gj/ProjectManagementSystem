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
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;

        public ProjectMembersController(
            IProjectMemberRepository projectMemberRepository,
            IProjectTaskRepository projectTaskRepository)
        {
            _projectMemberRepository = projectMemberRepository;
            _projectTaskRepository = projectTaskRepository;
        }

        private async Task<bool> UserCanManageProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false;

            var ownerId = await _projectMemberRepository.GetProjectOwnerIdAsync(projectId);
            if (!string.IsNullOrEmpty(ownerId) && ownerId == userId)
                return true;

            var member = await _projectMemberRepository.GetProjectMemberAsync(projectId, userId);
            return member?.Role == "Manager";
        }

        public async Task<IActionResult> Index(int projectId)
        {
            var members = await _projectMemberRepository.GetProjectMembersAsync(projectId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentMember = await _projectMemberRepository.GetProjectMemberAsync(projectId, userId);
            var isOwner = await _projectMemberRepository.GetProjectOwnerIdAsync(projectId) == userId;

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

            var success = await _projectMemberRepository.AddMemberByEmailAsync(projectId, email);
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

            await _projectMemberRepository.RemoveMemberAsync(projectId, userId);
            return RedirectToAction("Index", new { projectId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int projectId, string userId, string newRole)
        {
            if (!await UserCanManageProject(projectId))
                return Forbid();

            await _projectMemberRepository.ChangeMemberRoleAsync(projectId, userId, newRole);
            return RedirectToAction("Index", new { projectId });
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectMembersByTask(int taskId)
        {
            var task = await _projectTaskRepository.GetByIdAsync(taskId);
            if (task == null) return NotFound();

            var members = await _projectMemberRepository.GetProjectMembersAsync(task.ProjectId);

            var result = members.Select(m => new
            {
                id = m.User.Id,
                userName = m.User.UserName
            });

            return Json(result);
        }

        [HttpGet("List")]
        public async Task<IActionResult> List(int projectId)
        {
            try
            {
                var members = await _projectMemberRepository.GetProjectMembersAsync(projectId);

                var result = members.Select(m => new
                {
                    id = m.User.Id,
                    userName = m.User.UserName,
                    avatarUrl = m.User.AvatarUrl
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}