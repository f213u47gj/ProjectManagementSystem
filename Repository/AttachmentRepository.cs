using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AttachmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attachment>> GetAttachmentsByTaskIdAsync(int taskId)
        {
            return await _context.Attachments
                .Include(a => a.UploadedBy)
                .Include(a => a.ProjectTask)
                .Where(a => a.ProjectTaskId == taskId)
                .OrderByDescending(a => a.UploadedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Attachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _context.Attachments
                .Include(a => a.UploadedBy)
                .Include(a => a.ProjectTask)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);
        }

        public async Task<Attachment> AddAttachmentAsync(Attachment attachment)
        {
            attachment.UploadedAt = DateTime.UtcNow;
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserCanDeleteAttachment(int attachmentId, string userId, bool isManagerOrOwner)
        {
            var attachment = await _context.Attachments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            return attachment != null && (isManagerOrOwner || attachment.UploadedById == userId);
        }
    }
}