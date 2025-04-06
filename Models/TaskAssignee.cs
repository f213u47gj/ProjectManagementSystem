using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class TaskAssignee
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProjectTask")]
        public int ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; } = null!;

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }
}
