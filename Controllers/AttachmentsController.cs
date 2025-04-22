using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;

[Authorize]
public class AttachmentsController : Controller
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly ITaskAssigneeRepository _taskAssigneeRepository;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;

    public AttachmentsController(
        IAttachmentRepository attachmentRepository,
        IProjectTaskRepository taskRepository,
        IProjectMemberRepository projectMemberRepository,
        ITaskAssigneeRepository taskAssigneeRepository,
        UserManager<User> userManager,
        IWebHostEnvironment environment)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _projectMemberRepository = projectMemberRepository;
        _taskAssigneeRepository = taskAssigneeRepository;
        _userManager = userManager;
        _environment = environment;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(int taskId, IFormFile file)
    {
        try
        {
            // Валидация файла
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("Максимальный размер файла - 10 МБ");

            var allowedExtensions = new[] {
            ".pdf", ".docx", ".xlsx", ".jpg", ".png", ".txt",
            ".cs", ".py", ".html", ".js", ".cshtml", ".json", ".sql"
        };

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Допустимые форматы: " + string.Join(", ", allowedExtensions));

            // Проверка прав
            var task = await _taskRepository.GetTaskByIdAsync(taskId);
            if (task == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAssignee = await _taskAssigneeRepository.AssignmentExistsAsync(taskId, userId);
            var role = await _projectMemberRepository.GetUserRoleInProjectAsync(task.ProjectId, userId);

            if (role != "Owner" && role != "Manager" && !isAssignee)
                return Forbid();

            // Создание структуры папок
            var projectUploadsDir = Path.Combine(_environment.WebRootPath, "uploads", $"Project{task.ProjectId}");
            var taskUploadsDir = Path.Combine(projectUploadsDir, $"Task{taskId}");

            Directory.CreateDirectory(projectUploadsDir);
            Directory.CreateDirectory(taskUploadsDir);

            // Использование оригинального имени файла
            var fileName = file.FileName; // используем имя файла, переданное пользователем
            var filePath = Path.Combine(taskUploadsDir, fileName);

            // Сохранение файла
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                ProjectTaskId = taskId,
                FileUrl = $"/uploads/Project{task.ProjectId}/Task{taskId}/{fileName}",
                UploadedById = userId
            };

            await _attachmentRepository.AddAttachmentAsync(attachment);

            return Ok(new
            {
                id = attachment.Id,
                url = attachment.FileUrl,
                uploadDate = attachment.UploadedAt.ToString("dd.MM.yyyy HH:mm"),
                userId = attachment.UploadedById,
                canDelete = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ошибка сервера");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAttachments(int taskId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId);
        if (task == null) return NotFound();

        var attachments = await _attachmentRepository.GetAttachmentsByTaskIdAsync(taskId);
        var userId = _userManager.GetUserId(User);

        var role = await _projectMemberRepository.GetUserRoleInProjectAsync(task.ProjectId, userId);
        var isManagerOrOwner = role == "Owner" || role == "Manager";

        return Json(attachments.Select(a => new {
            id = a.Id,
            url = a.FileUrl,
            uploadDate = a.UploadedAt.ToString("dd.MM.yyyy HH:mm"),
            userId = a.UploadedById,
            userName = a.UploadedBy.UserName,
            canDelete = isManagerOrOwner || a.UploadedById == userId
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _attachmentRepository.GetAttachmentByIdAsync(id);
        if (attachment == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var role = await _projectMemberRepository.GetUserRoleInProjectAsync(
            attachment.ProjectTask.ProjectId, userId);

        var isManagerOrOwner = role == "Owner" || role == "Manager";
        var canDelete = await _attachmentRepository.UserCanDeleteAttachment(id, userId, isManagerOrOwner);

        if (!canDelete) return Forbid();

        await _attachmentRepository.DeleteAttachmentAsync(id);

        var filePath = Path.Combine(_environment.WebRootPath, attachment.FileUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> CanUpload(int taskId)
    {
        var canUpload = await CanUploadAttachment(taskId);
        return Json(new { canUpload });
    }

    private async Task<bool> CanUploadAttachment(int taskId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId);
        if (task == null) return false;

        var userId = _userManager.GetUserId(User);
        var isAssignee = await _taskAssigneeRepository.AssignmentExistsAsync(taskId, userId);
        var role = await _projectMemberRepository.GetUserRoleInProjectAsync(task.ProjectId, userId);

        return role == "Owner" || role == "Manager" || isAssignee;
    }

    private async Task<bool> CanDeleteAttachments(int taskId, string userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId);
        if (task == null) return false;

        var role = await _projectMemberRepository.GetUserRoleInProjectAsync(task.ProjectId, userId);
        return role == "Owner" || role == "Manager";
    }
}