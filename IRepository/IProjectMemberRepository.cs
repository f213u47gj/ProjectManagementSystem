using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.IRepositories
{
    public interface IProjectMemberRepository
    {
        Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(int projectId);
        Task<bool> AddMemberByEmailAsync(int projectId, string email);
        Task<bool> RemoveMemberAsync(int projectId, string userId);
        Task<bool> ChangeMemberRoleAsync(int projectId, string userId, string newRole);
        Task<string?> GetProjectOwnerIdAsync(int projectId);
        Task<ProjectMember?> GetProjectMemberAsync(int projectId, string userId); // ✅ Добавлено
        Task<string?> GetUserRoleAsync(int projectId, Guid userId);
    }

}
