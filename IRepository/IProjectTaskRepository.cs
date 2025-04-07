using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.IRepositories
{
    public interface IProjectTaskRepository
    {
        Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(int projectId);
        Task<ProjectTask?> GetTaskByIdAsync(int taskId);
        Task CreateTaskAsync(ProjectTask task);
        Task UpdateTaskAsync(ProjectTask task);
        Task DeleteTaskAsync(int taskId);
    }
}
