using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITaskCommentService
    {
        Task AddComentTask(AddTaskCommentRequest request);
    }
}
