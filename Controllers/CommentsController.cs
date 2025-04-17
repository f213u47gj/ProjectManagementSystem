using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IProjectTaskRepository _taskRepository;

        public CommentsController(
            ICommentRepository commentRepository,
            IProjectTaskRepository taskRepository)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
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
        public async Task<IActionResult> Delete(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != userId)
                return Forbid(); // Только автор может удалить

            await _commentRepository.DeleteAsync(commentId);
            return Ok();
        }

        [HttpGet("List")]
        public async Task<IActionResult> List(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return NotFound();

            var comments = await _commentRepository.GetByTaskIdAsync(taskId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = comments.Select(c => new
            {
                id = c.Id,
                content = c.Content,
                createdAt = c.CreatedAt,
                userId = c.User.Id,
                userName = c.User.UserName,
                avatarUrl = string.IsNullOrEmpty(c.User.AvatarUrl) ? "/img/default-avatar.png" : c.User.AvatarUrl,
                isMine = c.UserId == userId
            });



            return Json(result);
        }
    }
}
