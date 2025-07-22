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

        public async Task AcceptTaskAsync(TaskAssignee data)
        {
            await _context.TaskAssignees.AddAsync(data);
            _context.SaveChanges();
        }

        public Task CreateListTaskAssignee(List<TaskAssignee> taskAssignees)
        {
            _context.TaskAssignees.AddRange(taskAssignees);
            return _context.SaveChangesAsync();
        }

        public async Task CreateTaskAssignee(TaskAssignee taskAssignee)
        {
            _context.TaskAssignees.Add(taskAssignee);
            await _context.SaveChangesAsync();
        }

        public async Task<TaskAssignee?> GetTaskAssigneeAsync(Guid taskAssigneeId)
        {
            return await _context.TaskAssignees
                .FirstOrDefaultAsync(x => x.Id == taskAssigneeId
                    && x.IsActive);
        }

        public Task<TaskAssignee?> GetTaskAssigneeByTaskIdAndUserIDAsync(Guid taskId, Guid projectmemberId)
        {
            return _context.TaskAssignees
                .FirstOrDefaultAsync(x => x.RefId == taskId
                && x.ImplementerId == projectmemberId
                && x.Type == RefType.Task
                && x.IsActive);
        }

        public async Task<bool> IsTaskAssigneeExistsAsync(Guid taskId, Guid assignerId)
        {
            return await _context.TaskAssignees
                .AnyAsync(x => x.RefId == taskId 
                && x.ImplementerId == assignerId
                && x.Type == RefType.Task
                &&x.IsActive);
        }

        public Task ListAcceptTask(List<TaskAssignee> data)
        {
            _context.TaskAssignees.AddRange(data);
            return _context.SaveChangesAsync();
        }

        public async Task<List<TaskAssignee>> taskAssigneesAsync(Guid taskId)
        {
            var taskAssignees = await _context.TaskAssignees
                .Where(x => x.RefId == taskId && x.Type == RefType.Task && x.IsActive)
                .ToListAsync();
            return taskAssignees;
        }

        public async Task UpdateAsync(TaskAssignee taskAssignee)
        {
            _context.TaskAssignees.Update(taskAssignee);
            await _context.SaveChangesAsync();
        }
    }
}
