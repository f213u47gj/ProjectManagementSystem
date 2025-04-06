using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProjectTask")]
        public int ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; } = null!;

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [ForeignKey("User")]
        public string UploadedById { get; set; } = string.Empty;
        public User UploadedBy { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
