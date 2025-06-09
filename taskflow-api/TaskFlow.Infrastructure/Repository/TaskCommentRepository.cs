using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskCommentRepository : ITaskCommentRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskCommentRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddTaskCommentAysc(TaskComment data)
        {
            await _context.AddAsync(data);
             _context.SaveChanges();
        }
    }
}
