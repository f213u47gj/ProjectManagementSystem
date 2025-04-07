using ProjectManagementSystem.Models;
using System.Collections.Generic;

namespace ProjectManagementSystem.ViewModels.forProject
{
    public class ProjectBoardViewModel
    {
        public Project Project { get; set; } = null!;
        public Dictionary<string, List<ProjectTask>> GroupedTasks { get; set; } = new();
    }
}
