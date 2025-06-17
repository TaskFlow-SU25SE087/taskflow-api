using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskAssigneeRepository : ITaskAssigneeRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskAssigneeRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AcceptTaskAsync(Domain.Entities.TaskAssignee data)
        {
            await _context.TaskAssignees.AddAsync(data);
            _context.SaveChanges();
        }

        public async Task<TaskAssignee?> GetTaskAssigneeAsync(Guid taskAssigneeId)
        {
            return await _context.TaskAssignees
                .FirstOrDefaultAsync(x => x.Id == taskAssigneeId
                    && x.IsActive);
        }

        public async Task<bool> IsTaskAssigneeExistsAsync(Guid taskId, Guid assignerId)
        {
            return await _context.TaskAssignees
                .AnyAsync(x => x.RefId == taskId 
                && x.AssignerId == assignerId
                && x.Type == RefType.Task
                &&x.IsActive);
        }

        public async Task UpdateAsync(TaskAssignee taskAssignee)
        {
            _context.TaskAssignees.Update(taskAssignee);
            await _context.SaveChangesAsync();
        }
    }
}
