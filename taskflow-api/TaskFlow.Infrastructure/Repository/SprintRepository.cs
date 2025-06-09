using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class SprintRepository : ISprintRepository
    {
        private readonly TaskFlowDbContext _context;

        public SprintRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateSprintAsync(Sprint sprint)
        {
            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Sprint>> GetListPrintAsync(Guid projectId)
        {
            var result = await _context.Sprints
                .Where(p => p.ProjectId == projectId && p.IsActive)
                .OrderBy(p => p.StartDate) 
                .ToListAsync();
            return result;
        }

        public async Task UpdateSprintAsync(Sprint sprint)
        {
            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();
        }
    }
}
