using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class ProjectPart
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public string Name { get; set; } = string.Empty;
        public ProgrammingLanguage ProgrammingLanguage { get; set; } = ProgrammingLanguage.None;
        public Framework Framework { get; set; } = Framework.None;
        public RepoProvider? RepoProvider { get; set; }
        public string? RepoUrl { get; set; }
        public string? AccessToken { get; set; }
        public string? Branch { get; set; } = "main";
        public string? WebhookUrl { get; set; }
    }
}
