using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class SprintMeetingLogsRepository : ISprintMeetingLogsRepository
    {
        private readonly TaskFlowDbContext _context;
        private readonly AppTimeProvider _appTimeProvider;

        public SprintMeetingLogsRepository(TaskFlowDbContext context, AppTimeProvider appTimeProvider)
        {
            _context = context;
            _appTimeProvider = appTimeProvider;
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

        public async Task<SprintMeetingLog?> GetSprintMettingByID(Guid mettingID)
        {
            return await _context.SprintMeetingLogs
                .AsNoTracking()
                .Include(x => x.Sprint)
                .FirstOrDefaultAsync(x => x.Id == mettingID);
        }

        public async Task<List<SprintMeetingLog>> GetAllSprintMettingCanUpdate(Guid projectId, DateTime since)
        {
            return await _context.SprintMeetingLogs
                .AsNoTracking()
                .Include(x => x.Sprint)
                .Where(x => x.Sprint.ProjectId == projectId && x.CreatedAt >= since)
                .ToListAsync();
        }

        public async Task UpdateSprintMeetingLog(SprintMeetingLog sprintmeeting)
        {
            _context.SprintMeetingLogs.Update(sprintmeeting);
            await _context.SaveChangesAsync();
        }
    }
}
