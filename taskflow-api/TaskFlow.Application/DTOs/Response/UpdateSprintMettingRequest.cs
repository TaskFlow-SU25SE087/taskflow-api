using taskflow_api.TaskFlow.Application.DTOs.Common;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class UpdateSprintMettingRequest
    {
        public List<UnfinishedTaskResponse> UnfinishedTasks { get; set; } = new List<UnfinishedTaskResponse>();
        public string NextPlan { get; set; } = string.Empty;
    }
}
