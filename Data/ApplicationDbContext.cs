using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<TaskAssignee> TaskAssignees { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.ProjectTasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<TaskAssignee>()
                .HasKey(ta => new { ta.ProjectTaskId, ta.UserId });

            modelBuilder.Entity<TaskAssignee>()
                .HasOne(ta => ta.ProjectTask)
                .WithMany(t => t.Assignees)
                .HasForeignKey(ta => ta.ProjectTaskId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<TaskAssignee>()
                .HasOne(ta => ta.User)
                .WithMany()
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.NoAction); 
            
            modelBuilder.Entity<TaskHistory>()
                .HasOne(th => th.ProjectTask)
                .WithMany(t => t.History)
                .HasForeignKey(th => th.ProjectTaskId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ProjectTask)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.ProjectTaskId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.ProjectTask)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.ProjectTaskId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.UploadedBy)
                .WithMany()
                .HasForeignKey(a => a.UploadedById)
                .OnDelete(DeleteBehavior.NoAction);     
        }
    }
}
