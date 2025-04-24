using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<User> _userManager;

        public ProjectTasksController(
            IProjectTaskRepository taskRepository,
            IProjectMemberRepository memberRepository,
            IProjectRepository projectRepository,
            UserManager<User> userManager)
        {
            _taskRepository = taskRepository;
            _memberRepository = memberRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        private async Task<bool> UserHasRoleAsync(int projectId, params string[] allowedRoles)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
                return false;

            if (project.OwnerId == userId && allowedRoles.Contains("Owner"))
                return true;

            var members = await _memberRepository.GetProjectMembersAsync(projectId);
            var member = members.FirstOrDefault(m => m.UserId == userId);

            return member != null && allowedRoles.Contains(member.Role);
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

            if (!await UserHasRoleAsync(task.ProjectId, "Owner", "Manager"))
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

        public async Task<IActionResult> ViewTask(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            var userId = Guid.Parse(_userManager.GetUserId(User));
            var role = await _memberRepository.GetUserRoleAsync(task.ProjectId, userId);

            var viewModel = new ProjectTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                DueDate = task.DueDate,
                ProjectId = task.ProjectId,
                IsReadOnly = true
            };

            return PartialView("_TaskModal", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound("Задача не найдена");
            }

            if (!await UserHasRoleAsync(task.ProjectId, "Owner", "Manager", "Developer"))
            {
                return Forbid();
            }

            var validStatuses = new[] { "ToDo", "InProgress", "Done" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Недопустимый статус задачи");
            }

            task.Status = status;
            await _taskRepository.UpdateTaskAsync(task);

            return Ok();
        }
    }
}
