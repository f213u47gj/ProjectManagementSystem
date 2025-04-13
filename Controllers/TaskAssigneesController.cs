using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskAssigneesController : Controller
    {
        private readonly ITaskAssigneeRepository _taskAssigneeRepo;
        private readonly IProjectTaskRepository _projectTaskRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly UserManager<User> _userManager;

        public TaskAssigneesController(
            ITaskAssigneeRepository taskAssigneeRepo,
            IProjectTaskRepository projectTaskRepo,
            IProjectMemberRepository projectMemberRepo,
            UserManager<User> userManager)
        {
            _taskAssigneeRepo = taskAssigneeRepo;
            _projectTaskRepo = projectTaskRepo;
            _projectMemberRepo = projectMemberRepo;
            _userManager = userManager;
        }

        private async Task<bool> UserCanManageTask(int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false;

            var task = await _projectTaskRepo.GetByIdAsync(taskId);
            if (task == null) return false;

            var ownerId = await _projectMemberRepo.GetProjectOwnerIdAsync(task.ProjectId);
            if (!string.IsNullOrEmpty(ownerId) && ownerId == userId)
                return true;

            var member = await _projectMemberRepo.GetProjectMemberAsync(task.ProjectId, userId);
            return member?.Role == "Manager";
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAssignee([FromBody] AssigneeRequest request)
        {
            try
            {
                Console.WriteLine($"Получен запрос: TaskId={request.TaskId}, UserId={request.UserId}");

                if (!await UserCanManageTask(request.TaskId))
                    return Json(new { success = false, message = "Нет прав для выполнения этой операции" });

                if (await _taskAssigneeRepo.AssignmentExistsAsync(request.TaskId, request.UserId))
                    return Json(new { success = false, message = "Этот пользователь уже назначен на задачу" });

                var result = await _taskAssigneeRepo.AddAssigneeAsync(request.TaskId, request.UserId);

                if (!result)
                    return Json(new { success = false, message = "Не удалось назначить исполнителя" });

                var assignee = await _userManager.FindByIdAsync(request.UserId);

                return Json(new
                {
                    success = true,
                    assignee = new
                    {
                        assignee.Id,
                        assignee.UserName,
                        assignee.AvatarUrl
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class AssigneeRequest
        {
            public int TaskId { get; set; }
            public string UserId { get; set; }
        }

        [HttpPost("Remove")]
        public async Task<IActionResult> RemoveAssignee([FromBody] RemoveAssigneeRequest request)
        {
            try
            {
                Console.WriteLine($"Запрос на удаление: TaskId={request.TaskId}, UserId={request.UserId}");

                if (!await UserCanManageTask(request.TaskId))
                    return Json(new { success = false, message = "Нет прав для выполнения этой операции" });

                var result = await _taskAssigneeRepo.RemoveAssigneeAsync(request.TaskId, request.UserId);

                return Json(new
                {
                    success = result,
                    message = result ? "Исполнитель успешно удален" : "Ошибка при удалении исполнителя"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class RemoveAssigneeRequest
        {
            public int TaskId { get; set; }
            public string UserId { get; set; }
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetAssignees(int taskId)
        {
            try
            {
                var assignees = await _taskAssigneeRepo.GetAssigneesForTaskAsync(taskId);

                // Преобразуем данные в правильный формат
                var result = assignees.Select(a => new
                {
                    Id = a.Id,
                    UserName = a.UserName,
                    AvatarUrl = a.AvatarUrl ?? "/img/default-avatar.png"
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("AvailableMembers")]
        public async Task<IActionResult> GetAvailableMembers(int taskId)
        {
            try
            {
                var task = await _projectTaskRepo.GetByIdAsync(taskId);
                if (task == null)
                    return Json(new { success = false, message = "Задача не найдена" });

                // Получаем всех участников проекта
                var allMembers = await _projectMemberRepo.GetProjectMembersAsync(task.ProjectId);

                // Получаем уже назначенных исполнителей
                var assignedUserIds = (await _taskAssigneeRepo.GetAssigneesForTaskAsync(taskId))
                    .Select(a => a.Id)
                    .ToList();

                // Фильтруем участников
                var availableMembers = allMembers
                    .Where(m => !assignedUserIds.Contains(m.UserId))
                    .Select(m => new
                    {
                        Id = m.UserId,
                        UserName = m.User?.UserName ?? m.User?.Email ?? $"User {m.UserId}",
                        AvatarUrl = m.User?.AvatarUrl ?? "/img/default-avatar.png",
                        Role = m.Role
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    members = availableMembers
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
