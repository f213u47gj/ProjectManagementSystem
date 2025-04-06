using Microsoft.AspNetCore.Identity;
using ProjectManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public ICollection<ProjectMember> Projects { get; set; } = new List<ProjectMember>();
        public ICollection<TaskAssignee> AssignedTasks { get; set; } = new List<TaskAssignee>();
    }
}