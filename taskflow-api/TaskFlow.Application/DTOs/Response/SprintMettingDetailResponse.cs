using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class SprintMettingDetailResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SprintId { get; set; }

        public string SprintName { get; set; }

        public List<UnfinishedTaskDto> UnfinishedTasks { get; set; } = new List<UnfinishedTaskDto>();
        public List<TaskCompleteDTO> CompletedTasks { get; set; } = new List<TaskCompleteDTO>();
        public string NextPlan { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;

    }
}
