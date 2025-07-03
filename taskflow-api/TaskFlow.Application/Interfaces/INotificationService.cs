namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskUpdateAsync(Guid userId, Guid projectId, Guid taskId, string message);
        Task NotifyProjectMemberChangeAsync(Guid projectId, string message);
    }
}
