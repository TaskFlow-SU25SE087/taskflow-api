using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITaskProjectService
    {
        Task<TaskProject> AddTask(AddTaskRequest request, Guid ProjectId);
        Task<TaskProject> UpdateTask(UpdateTaskRequest request, Guid TaskId);
        Task<bool> DeleteTask(Guid taskId);
        Task<List<TaskProjectResponse>> GetAllTask(Guid projectId);
        Task AddTagForTask(Guid ProjectId, Guid TaskId, Guid TagId);
        Task userAcceptTask(Guid TaskId);
        Task AssignTaskToUser(Guid TaskId, Guid AssignerId);
        Task LeaveTask(Guid TaskAssigneeId, string Reason);
        Task RevokeTaskAssignment(Guid TaskAssigneeId, string Reason);
    }
}
