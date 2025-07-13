using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectPartResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;
        public ProgrammingLanguage ProgrammingLanguage { get; set; } = ProgrammingLanguage.None;
        public Framework Framework { get; set; } = Framework.None;

        public RepoProvider? RepoProvider { get; set; }
        public string? RepoUrl { get; set; }

        public Guid? OwnerId { get; set; }
        public string? OwnerName { get; set; } = string.Empty;
        public string? AvatrarUrl { get; set; } = string.Empty;
    }
}