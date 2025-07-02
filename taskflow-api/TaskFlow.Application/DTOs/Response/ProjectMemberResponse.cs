using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectMemberResponse
    {
        public Guid ProjectMemberId { get; set; }
        public Guid ProjectId { get; set; }
        public ProjectRole Role { get; set; }
    }
}
