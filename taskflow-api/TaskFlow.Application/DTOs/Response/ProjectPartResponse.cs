using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectPartResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ProgrammingLanguage ProgrammingLanguage { get; set; } = ProgrammingLanguage.None;
        public Framework Framework { get; set; } = Framework.None;
        public RepoProvider? RepoProvider { get; set; }
        public string? RepoUrl { get; set; }
    }
}
