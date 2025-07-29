using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class SprintMeetingResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
