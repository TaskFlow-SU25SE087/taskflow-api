namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateProjectRequest
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
