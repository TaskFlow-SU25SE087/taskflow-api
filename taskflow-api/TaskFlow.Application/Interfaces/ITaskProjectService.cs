using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITaskProjectService
    {
        Task<TaskProject> AddTask(AddTaskRequest request);
        Task<TaskProject> UpdateTask(UpdateTaskRequest request);
        Task<bool> DeleteTask(Guid taskId);
    }
}
