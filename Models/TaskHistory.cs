using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class TaskHistory
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProjectTask")]
        public int ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; } = null!;

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public string ChangeType { get; set; } = string.Empty; // "status_changed", "assignee_added", etc.
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
