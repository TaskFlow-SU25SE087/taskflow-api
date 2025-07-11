using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CommitJobMessage
    {
        public Guid CommitRecordId { get; set; }
        public TypeJobRabbit JobType { get; set; } = TypeJobRabbit.scanCode;
        public string RepoFullName { get; set; } = string.Empty;
        public string CommitId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
