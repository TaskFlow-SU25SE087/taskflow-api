using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;


namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description  { get; set; } = string.Empty;
        [Required]
        public string Semester { get; set; } = null!;
        public Guid TermId { get; set; }
        public Term Term { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = false;
        public List<ProjectMember> Members { get; set; } = new();
        public List<Board> Boards { get; set; } = new List<Board>();
        public List<Sprint> Sprints { get; set; } = new List<Sprint>();
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<ProjectPart> ProjectParts { get; set; } = new List<ProjectPart>();
    }
}
