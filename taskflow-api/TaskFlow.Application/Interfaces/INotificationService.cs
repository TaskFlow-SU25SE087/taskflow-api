namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskUpdateAsync(Guid userId, Guid taskId, string message);
    }
}
