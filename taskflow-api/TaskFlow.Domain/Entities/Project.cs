using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public List<ProjectMember> Members { get; set; } = new();
        public List<Board> Boards { get; set; } = new List<Board>();
        public List<Sprint> Sprints { get; set; } = new List<Sprint>();
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
    }
}
