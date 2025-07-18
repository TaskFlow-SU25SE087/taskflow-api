namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskUpdateAsync(Guid userId, Guid projectId, Guid taskId, string message);
        Task NotifyProjectMemberChangeAsync(Guid projectId, string message);
        Task NotifyTaskBoardChangeAsync(Guid projectId, Guid taskId, string oldBoardName, string newBoardName, List<Guid> userIds);
        Task<List<taskflow_api.TaskFlow.Domain.Entities.Notification>> GetUserNotificationsAsync(Guid userId);
    }
}
