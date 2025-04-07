using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Repositories;
using ProjectManagementSystem.ViewModels.Tasks;
using ProjectManagementSystem.Views.ProjectTasks;
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
        [Route("ProjectTasks/Create")]
        public async Task<IActionResult> Create([FromBody] ProjectTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = new ProjectTask
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                DueDate = model.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.CreateTaskAsync(task);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProjectTask updatedTask)
        {
            var existingTask = await _taskRepository.GetByIdAsync(updatedTask.Id);
            if (existingTask == null)
            {
                return NotFound("Задача не найдена");
            }

            ModelState.Remove("Project");
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные");
            }

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.Status = updatedTask.Status;
            existingTask.DueDate = updatedTask.DueDate;

            await _taskRepository.UpdateTaskAsync(existingTask);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrUpdate(ProjectTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id == 0)
            {
                var task = new ProjectTask
                {
                    ProjectId = model.ProjectId,
                    Title = model.Title,
                    Description = model.Description,
                    Status = model.Status,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = model.DueDate
                };

                await _taskRepository.CreateTaskAsync(task);
            }
            else
            {
                var task = await _taskRepository.GetTaskByIdAsync(model.Id);
                if (task == null || task.ProjectId != model.ProjectId)
                    return NotFound();

                task.Title = model.Title;
                task.Description = model.Description;
                task.Status = model.Status;
                task.DueDate = model.DueDate;

                await _taskRepository.UpdateTaskAsync(task);
            }

            return Json(new { success = true });
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
        public async Task<IActionResult> Get(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            return Json(new
            {
                id = task.Id,
                title = task.Title,
                description = task.Description,
                status = task.Status,
                dueDate = task.DueDate?.ToString("yyyy-MM-dd")
            });
        }
    }
}
