using ProjectManagementSystem.Models;
using System.Security.Claims;

namespace ProjectManagementSystem.IRepositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetUserProjectsAsync(ClaimsPrincipal user);
        Task<Project?> GetProjectByIdAsync(int id);
        Task<Project?> GetProjectWithBoardAsync(int id);
        Task<bool> CreateProjectAsync(Project project, ClaimsPrincipal user);
        Task<bool> AddMemberByEmailAsync(int projectId, string email);
        Task UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(Project project);
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<IEnumerable<ProjectMember>> GetProjectsByUserIdAsync(string userId);
    }
}
