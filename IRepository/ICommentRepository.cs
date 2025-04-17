using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.IRepositories
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
        Task DeleteAsync(int commentId);
        Task<Comment?> GetByIdAsync(int commentId);
        Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId);
    }
}