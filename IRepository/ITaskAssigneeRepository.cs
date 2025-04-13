using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.IRepositories
{
    public interface ITaskAssigneeRepository
    {
        Task<bool> AssignmentExistsAsync(int taskId, string userId);
        Task<bool> AddAssigneeAsync(int taskId, string userId);
        Task<bool> RemoveAssigneeAsync(int taskId, string userId);
        Task<IEnumerable<User>> GetAssigneesForTaskAsync(int taskId);
        Task<IEnumerable<User>> GetAvailableMembersForTaskAsync(int taskId);
        Task<User?> GetAssigneeInfoAsync(string userId);
    }
}
