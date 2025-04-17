using Microsoft.AspNetCore.Authorization;
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
    public class CommentsController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IProjectTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _memberRepository;

        public CommentsController(
            ICommentRepository commentRepository,
            IProjectTaskRepository taskRepository,
            IProjectRepository projectRepository,
            IProjectMemberRepository memberRepository)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _memberRepository = memberRepository;
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] int taskId, [FromForm] string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(content) || userId == null)
                return BadRequest();

            var comment = new Comment
            {
                ProjectTaskId = taskId,
                Content = content,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            return Ok();
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] int commentId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                {
                    return NotFound(new { message = "Комментарий не найден" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var task = await _taskRepository.GetTaskByIdAsync(comment.ProjectTaskId);
                var project = await _projectRepository.GetProjectByIdAsync(task.ProjectId);

                if (!await CheckDeletePermission(comment, userId, project))
                    return Forbid();

                bool success = await _commentRepository.DeleteAsync(commentId);
                return success ? Ok() : StatusCode(500);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("List")]
        public async Task<IActionResult> List(int taskId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                // Получаем задачу и проверяем доступ
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null) return NotFound("Задача не найдена");

                var project = await _projectRepository.GetProjectByIdAsync(task.ProjectId);
                if (project == null) return NotFound("Проект не найден");

                // Проверяем, является ли пользователь участником проекта
                var isMember = project.OwnerId == userId ||
                              await _memberRepository.IsUserInProjectAsync(project.Id, userId);
                if (!isMember) return Forbid();

                // Получаем комментарии с включением данных пользователя
                var comments = await _commentRepository.GetByTaskIdWithUserAsync(taskId);

                var result = new List<object>();
                foreach (var comment in comments)
                {
                    // Проверяем права на удаление для каждого комментария
                    var canDelete = await CheckDeletePermission(comment, userId, project);

                    result.Add(new
                    {
                        id = comment.Id,
                        content = comment.Content,
                        createdAt = comment.CreatedAt,
                        userId = comment.UserId,
                        userName = comment.User?.UserName ?? "Удаленный пользователь",
                        avatarUrl = comment.User?.AvatarUrl ?? "/images/default-avatar.png",
                        isMine = comment.UserId == userId,
                        canDelete
                    });
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private async Task<bool> CheckDeletePermission(Comment comment, string currentUserId, Project project)
        {
            // Владелец может удалять любые комментарии
            if (project.OwnerId == currentUserId) return true;

            // Автор может удалять свои комментарии
            if (comment.UserId == currentUserId) return true;

            // Для менеджеров
            var currentUserRole = await _memberRepository.GetUserRoleInProjectAsync(project.Id, currentUserId);
            if (currentUserRole == "Manager")
            {
                var commentAuthorRole = await _memberRepository.GetUserRoleInProjectAsync(project.Id, comment.UserId);
                return commentAuthorRole == "Member"; // Менеджер может удалять комментарии участников
            }

            return false;
        }
    }
}