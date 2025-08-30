namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class LogChangeContext
    {
        public Guid? SprintId { get; set; } = null;
        public Guid? TaskProjectID { get; set; } = null;
    }
}
