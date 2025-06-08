using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskProjectRepository : ITaskProjectRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskProjectRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddTaskAsync(TaskProject task)
        {
             _context.TaskProjects.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task<TaskProject?> GetTaskByIdAsync(Guid id)
        {
            var task = await _context.TaskProjects
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            return task;
        }

        public async Task UpdateTaskAsync(TaskProject task)
        {
            _context.TaskProjects.Update(task);
            await _context.SaveChangesAsync();
        }
    }
}
