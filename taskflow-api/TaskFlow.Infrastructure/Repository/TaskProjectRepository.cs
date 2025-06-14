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

        public async Task<List<TaskProject>> GetAllTaskProjectAsync(Guid projectId)
        {
            var tasks = await _context.TaskProjects
                .Where(t => t.ProjectId == projectId && t.IsActive)
                .Include(t => t.Issues)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Board)
                .Include(t => t.TaskAssignees)
                    .ThenInclude(ta => ta.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .ToListAsync();
            return tasks;
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
