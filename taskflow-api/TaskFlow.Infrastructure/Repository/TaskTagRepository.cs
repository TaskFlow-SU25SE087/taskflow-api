using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskTagRepository : ITaskTagRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskTagRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddTaskTagAsync(TaskTag taskTag)
        {
            await _context.TaskTags.AddAsync(taskTag);
            await _context.SaveChangesAsync();
        }

        public async Task<TaskTag?> GetTaskTagAsync(Guid taskId, Guid tagId)
        {
            return await _context.FindAsync<TaskTag>(taskId, tagId);
        }

        public async Task RemoveTaskTagAsync(Guid taskId, Guid tagId)
        {
            var taskTag = await _context.FindAsync<TaskTag>(taskId, tagId);
            if (taskTag != null)
            {
                _context.TaskTags.Remove(taskTag);
                await _context.SaveChangesAsync();
            }
        }
    }
}
