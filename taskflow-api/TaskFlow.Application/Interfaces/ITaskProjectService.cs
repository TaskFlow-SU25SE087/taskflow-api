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
        Task AddTagForTask(Guid TaskId, Guid TagId);
        Task userAcceptTask(Guid ProjectId, Guid TaskId);
        Task AssignTaskToUser(Guid TaskId, Guid ProjectId, AssignTaskRequest request);
        Task LeaveTask(Guid ProjectID, Guid TaskId, AssignmentReasonRequest Reason);
        Task RevokeTaskAssignment(Guid ProjectId, Guid TaskId, RemoveAssignmentReasonRequest request);
        Task SubmitTaskCompletion(Guid Project, Guid taskId, CompleteTaskRequest request);
    }
}
