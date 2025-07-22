using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITaskCommentService
    {
        Task AddComentTask(Guid ProjectId, Guid TaskId, AddTaskCommentRequest request);
    }
}
