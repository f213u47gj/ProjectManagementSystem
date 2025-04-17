using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Repositories
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectMemberRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(int projectId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<bool> AddMemberByEmailAsync(int projectId, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var exists = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id);

            if (exists) return true; // уже участник

            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = user.Id,
                Role = "Member"
            };

            _context.ProjectMembers.Add(newMember);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null) return false;

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeMemberRoleAsync(int projectId, string userId, string newRole)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null) return false;

            member.Role = newRole;
            _context.ProjectMembers.Update(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetProjectOwnerIdAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            return project?.OwnerId;
        }

        public async Task<ProjectMember?> GetProjectMemberAsync(int projectId, string userId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task<string?> GetUserRoleAsync(int projectId, Guid userId)
        {
            string userIdString = userId.ToString();

            return await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.UserId == userIdString)
                .Select(pm => pm.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsUserInProjectAsync(int projectId, string userId)
        {
            return await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId) ||
                   await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        }

        public async Task<string?> GetUserRoleInProjectAsync(int projectId, string userId)
        {
            if (await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId))
                return "Owner";

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            return member?.Role;
        }
    }
}
