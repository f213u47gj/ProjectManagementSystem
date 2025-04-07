using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels.Tasks;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ProjectTasksController : Controller
    {
        private readonly IProjectTaskRepository _taskRepository;
        private readonly IProjectMemberRepository _memberRepository;

        public ProjectTasksController(
            IProjectTaskRepository taskRepository,
            IProjectMemberRepository memberRepository)
        {
            _taskRepository = taskRepository;
            _memberRepository = memberRepository;
        }

        private async Task<bool> UserIsMemberAsync(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var members = await _memberRepository.GetProjectMembersAsync(projectId);
            return members.Any(m => m.UserId == userId);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            if (!await UserIsMemberAsync(model.ProjectId))
                return Forbid();

            var task = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                ProjectId = model.ProjectId,
                Status = model.Status,
                DueDate = model.DueDate
            };

            await _taskRepository.CreateTaskAsync(task);
            return Ok(task); // Можно вернуть task.Id, если нужно
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] EditTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            var existing = await _taskRepository.GetTaskByIdAsync(model.Id);
            if (existing == null)
                return NotFound();

            if (!await UserIsMemberAsync(existing.ProjectId))
                return Forbid();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Status = model.Status ?? "todo";
            existing.DueDate = model.DueDate;

            await _taskRepository.UpdateTaskAsync(existing);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            if (!await UserIsMemberAsync(task.ProjectId))
                return Forbid();

            await _taskRepository.DeleteTaskAsync(id);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            if (!await UserIsMemberAsync(task.ProjectId))
                return Forbid();

            return Json(task);
        }
    }
}
