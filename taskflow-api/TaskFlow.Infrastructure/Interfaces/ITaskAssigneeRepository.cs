using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITaskAssigneeRepository
    {
        Task AcceptTaskAsync(TaskAssignee data);
        Task CreateListTaskAssignee(List<TaskAssignee> taskAssignees);
        Task<bool> IsTaskAssigneeExistsAsync(Guid taskId, Guid implement);
        Task<TaskAssignee?> GetTaskAssigneeAsync(Guid taskAssigneeId);
        Task<TaskAssignee?> GetTaskAssigneeByTaskIdAndUserIDAsync(Guid taskId, Guid projectmemberId);
        Task UpdateAsync(TaskAssignee taskAssignee);
        Task<List<TaskAssignee>> taskAssigneesAsync(Guid taskId);
    }
}
