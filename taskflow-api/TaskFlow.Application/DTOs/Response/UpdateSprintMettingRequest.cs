using taskflow_api.TaskFlow.Application.DTOs.Common;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class UpdateSprintMettingRequest
    {
        public List<UnfinishedTaskDto> UnfinishedTasks { get; set; } = new List<UnfinishedTaskDto>();
        public string NextPlan { get; set; } = string.Empty;
    }
}
