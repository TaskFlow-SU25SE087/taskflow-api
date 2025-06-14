using System.Threading.Tasks;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITaskTagRepository
    {
        Task AddTaskTagAsync(TaskTag taskTag);
        Task<TaskTag> GetTaskTagAsync(Guid taskId, Guid tagId);
    }
}
