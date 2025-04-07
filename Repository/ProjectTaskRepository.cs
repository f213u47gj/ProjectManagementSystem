using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Repositories
{
    public class ProjectTaskRepository : IProjectTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _context.ProjectTasks
                .Include(t => t.Assignees).ThenInclude(a => a.User)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(int taskId)
        {
            return await _context.ProjectTasks
                .Include(t => t.Assignees).ThenInclude(a => a.User)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task CreateTaskAsync(ProjectTask task)
        {
            task.CreatedAt = DateTime.UtcNow;
            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(ProjectTask updatedTask)
        {
            var existingTask = await _context.ProjectTasks.FindAsync(updatedTask.Id);
            if (existingTask == null) return;

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.Status = updatedTask.Status;
            existingTask.DueDate = updatedTask.DueDate;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task != null)
            {
                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ProjectTask?> GetByIdAsync(int id)
        {
            return await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == id);
        }

    }
}
