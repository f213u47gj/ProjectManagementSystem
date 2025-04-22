using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Repositories
{
    public class TaskAssigneeRepository : ITaskAssigneeRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskAssigneeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AssignmentExistsAsync(int taskId, string userId)
        {
            return await _context.TaskAssignees
                .AnyAsync(ta => ta.ProjectTaskId == taskId && ta.UserId == userId);
        }

        public async Task<bool> AddAssigneeAsync(int taskId, string userId)
        {
            if (await _context.TaskAssignees.AnyAsync(ta => ta.ProjectTaskId == taskId && ta.UserId == userId))
                return false;

            var taskAssignee = new TaskAssignee
            {
                ProjectTaskId = taskId,
                UserId = userId,
            };

            _context.TaskAssignees.Add(taskAssignee);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveAssigneeAsync(int taskId, string userId)
        {
            var assignment = await _context.TaskAssignees
                .FirstOrDefaultAsync(ta => ta.ProjectTaskId == taskId && ta.UserId == userId);

            if (assignment == null) return false;

            _context.TaskAssignees.Remove(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<User>> GetAssigneesForTaskAsync(int taskId)
        {
            return await _context.TaskAssignees
                .Where(ta => ta.ProjectTaskId == taskId)
                .Include(ta => ta.User)
                .Select(ta => ta.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAvailableMembersForTaskAsync(int taskId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return new List<User>();

            var assignedUserIds = await _context.TaskAssignees
                .Where(ta => ta.ProjectTaskId == taskId)
                .Select(ta => ta.UserId)
                .ToListAsync();

            return await _context.ProjectMembers
                .Where(pm => pm.ProjectId == task.ProjectId && !assignedUserIds.Contains(pm.UserId))
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToListAsync();
        }

        public async Task<User?> GetAssigneeInfoAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsUserAssignedToAnyTaskInProject(int projectId, string userId)
        {
            return await _context.TaskAssignees
                .AnyAsync(ta => ta.UserId == userId && ta.ProjectTask.ProjectId == projectId);
        }

    }
}
