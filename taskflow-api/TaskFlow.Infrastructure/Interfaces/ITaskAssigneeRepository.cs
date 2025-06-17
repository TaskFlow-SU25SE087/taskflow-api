using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITaskAssigneeRepository
    {
        Task AcceptTaskAsync(TaskAssignee data);
        Task<bool> IsTaskAssigneeExistsAsync(Guid taskId, Guid assignerId);
        Task<TaskAssignee?> GetTaskAssigneeAsync(Guid taskAssigneeId);
        Task UpdateAsync(TaskAssignee taskAssignee);
    }
}
