using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.IRepositories
{
    public interface IAttachmentRepository
    {
        Task<IEnumerable<Attachment>> GetAttachmentsByTaskIdAsync(int taskId);
        Task<Attachment?> GetAttachmentByIdAsync(int attachmentId);
        Task<Attachment> AddAttachmentAsync(Attachment attachment);
        Task<bool> DeleteAttachmentAsync(int attachmentId);
        Task<bool> UserCanDeleteAttachment(int attachmentId, string userId, bool isManagerOrOwner);
    }
}