using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description  { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdate { get; set; }
        public List<ProjectMember> Members { get; set; } = new();
        public List<Board> Boards { get; set; } = new List<Board>();
        public List<Sprint> Sprints { get; set; } = new List<Sprint>();
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
    }
}
