using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskIssueRepository : ITaskIssueRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskIssueRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateTaskIssueAsync(Issue data)
        {
            await _context.Issues.AddAsync(data);
            await _context.SaveChangesAsync();
        }
    }
}
