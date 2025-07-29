using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class SprintMeetingLogsRepository : ISprintMeetingLogsRepository
    {
        private readonly TaskFlowDbContext _context;

        public SprintMeetingLogsRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateMetting(SprintMeetingLog data)
        {
            _context.SprintMeetingLogs.Add(data);
             await _context.SaveChangesAsync();
        }

        public async Task<List<SprintMeetingResponse>> GetAllSprintMetting(Guid projectId)
        {
            return await _context.SprintMeetingLogs
                .AsNoTracking()
                .Where(x => x.Sprint.ProjectId == projectId)
                .Select(x => new SprintMeetingResponse
                {
                    Id = x.Id,
                    SprintId = x.SprintId,
                    SprintName = x.Sprint.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();
        }
    }
}
