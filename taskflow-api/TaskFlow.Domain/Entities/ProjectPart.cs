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
        public string? WebhookUrl { get; set; }

        public Guid? UserGitHubTokenId { get; set; }
        public UserGitHubToken? UserGitHubToken { get; set; }


        public List<CommitRecord> CommitRecords { get; set; } = new List<CommitRecord>();
        public List<GitMember> GitMembers { get; set; } = new List<GitMember>();
    }
}
