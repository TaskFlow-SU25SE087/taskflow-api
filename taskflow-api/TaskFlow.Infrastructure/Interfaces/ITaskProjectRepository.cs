using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITaskProjectRepository
    {
        Task AddTaskAsync(TaskProject task);
        Task<TaskProject?> GetTaskByIdAsync(Guid id);
        Task UpdateTaskAsync(TaskProject task);
        Task<List<TaskProjectResponse>> GetAllTaskProjectAsync(Guid projectId);
    }
}
