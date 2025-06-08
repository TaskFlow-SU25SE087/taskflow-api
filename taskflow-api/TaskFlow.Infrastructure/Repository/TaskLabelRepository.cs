using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskLabelRepository : ITaskLabelRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskLabelRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddTaskLabelAsync(TaskLabels taskLabel)
        {
            await _context.TaskLabels.AddAsync(taskLabel);
            await _context.SaveChangesAsync();
        }
    }
}
