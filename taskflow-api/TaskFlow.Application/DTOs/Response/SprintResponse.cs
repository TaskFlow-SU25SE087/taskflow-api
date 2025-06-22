using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class SprintResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SprintStatus Status { get; set; } = SprintStatus.NotStarted;
        public string? Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
