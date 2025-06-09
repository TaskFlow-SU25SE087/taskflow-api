using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITaskCommentRepository
    {
        Task AddTaskCommentAysc(TaskComment data);
    }
}
