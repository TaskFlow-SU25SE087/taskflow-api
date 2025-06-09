using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TaskCommentService : Interfaces.ITaskCommentService
    {
        private readonly ITaskCommentRepository _taskCommentRepository;

        public TaskCommentService(ITaskCommentRepository taskCommentRepository)
        {
            _taskCommentRepository = taskCommentRepository;
        }

        public async Task AddComentTask(AddTaskCommentRequest request)
        {
            var newTaskComment = new TaskComment
            {
                TaskId = request.TaskId,
                Commenter = request.Commenter,
                Content = request.Content,
                CreateAt = DateTime.Now,
            };
            await _taskCommentRepository.AddTaskCommentAysc(newTaskComment);
        }
    }
}
