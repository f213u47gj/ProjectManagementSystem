using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels.forProject;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<User> _userManager;

        public ProjectsController(IProjectRepository projectRepository, UserManager<User> userManager)
        {
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _projectRepository.GetUserProjectsAsync(User);
            return View(projects);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description
            };

            var success = await _projectRepository.CreateProjectAsync(project, User);
            if (!success)
            {
                ModelState.AddModelError("", "Ошибка при создании проекта.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Board(int id)
        {
            var project = await _projectRepository.GetProjectWithBoardAsync(id);
            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isMember = project.Members.Any(m => m.UserId == userId) || project.OwnerId == userId;
            if (!isMember)
                return Forbid();

            var groupedTasks = project.ProjectTasks
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new ProjectBoardViewModel
            {
                Project = project,
                GroupedTasks = groupedTasks
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isMember = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
            if (!isMember)
                return Forbid();

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetails(Project updated)
        {
            var project = await _projectRepository.GetProjectByIdAsync(updated.Id);
            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (project.OwnerId != userId) return Forbid();

            project.Name = updated.Name;
            project.Description = updated.Description;
            await _projectRepository.UpdateProjectAsync(project);

            return RedirectToAction("Details", new { id = project.Id });

        }
    }
}
