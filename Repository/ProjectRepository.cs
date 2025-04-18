using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using System.Security.Claims;

namespace ProjectManagementSystem.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Project>> GetUserProjectsAsync(ClaimsPrincipal user)
        {
            var currentUser = await _userManager.GetUserAsync(user);
            return await _context.Projects
                .Include(p => p.Owner)
                .Where(p => p.OwnerId == currentUser.Id || p.Members.Any(m => m.UserId == currentUser.Id))
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetProjectWithBoardAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Members).ThenInclude(m => m.User)
                .Include(p => p.ProjectTasks)
                    .ThenInclude(t => t.Assignees).ThenInclude(a => a.User)
                .Include(p => p.ProjectTasks)
                    .ThenInclude(t => t.Comments).ThenInclude(c => c.User)
                .Include(p => p.ProjectTasks)
                    .ThenInclude(t => t.Attachments).ThenInclude(a => a.UploadedBy)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CreateProjectAsync(Project project, ClaimsPrincipal user)
        {
            var currentUser = await _userManager.GetUserAsync(user);
            if (currentUser == null) return false;

            project.OwnerId = currentUser.Id;
            project.CreatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddMemberByEmailAsync(int projectId, string email)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return false;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var alreadyMember = project.Members.Any(m => m.UserId == user.Id);
            if (alreadyMember) return false;

            project.Members.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UserHasAccessAsync(int projectId, ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null) return false;

            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            return project != null && (project.OwnerId == user.Id || project.Members.Any(m => m.UserId == user.Id));
        }

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(Project project)
        {
            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == project.Id)
                .ToListAsync();
            _context.ProjectMembers.RemoveRange(members);

            var tasks = await _context.ProjectTasks
                .Where(t => t.ProjectId == project.Id)
                .ToListAsync();
            _context.ProjectTasks.RemoveRange(tasks);

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectMember>> GetProjectsByUserIdAsync(string userId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => pm.UserId == userId)
                .ToListAsync();
        }
    }
}
