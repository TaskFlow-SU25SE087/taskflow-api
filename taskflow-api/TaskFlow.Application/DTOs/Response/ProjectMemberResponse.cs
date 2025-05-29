using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectMemberResponse
    {
        public Guid ID { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public ProjectRole Role { get; set; }
    }
}
