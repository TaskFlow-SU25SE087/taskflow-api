using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
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

        public async Task<List<SprintResponse>> GetListPrintAsync(Guid projectId)
        {
            var result = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .Select(s => new SprintResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                })
                .ToListAsync();
            return result;
        }

        public Task<Sprint?> GetSprintByIdAsync(Guid sprintId)
        {
            return _context.Sprints
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(s => s.Id == sprintId);
        }

        public async Task UpdateSprintAsync(Sprint sprint)
        {
            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();
        }
    }
}
