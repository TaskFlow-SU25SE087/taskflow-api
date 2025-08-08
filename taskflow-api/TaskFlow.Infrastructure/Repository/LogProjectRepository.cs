using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class LogProjectRepository : ILogProjectRepository
    {
        private readonly TaskFlowDbContext _context;

        public LogProjectRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectLogResponse>> AllLogPrj(Guid projectId)
        {
            return await _context.LogProjects
                .Where(log => log.ProjectId == projectId)
                .Include(log => log.ProjectMember)
                    .ThenInclude(pm => pm.User)
                .OrderByDescending(log => log.CreatedAt)
                .Select(log => new ProjectLogResponse
                {
                    Id = log.Id,
                    ProjectMemberId = log.ProjectMemberId,
                    FullName = log.ProjectMember.User.FullName,
                    Avatar = log.ProjectMember.User.Avatar!,
                    ActionType = log.ActionType,
                    FieldChanged = log.FieldChanged,
                    OldValue = log.OldValue,
                    NewValue = log.NewValue,
                    Description = log.Description,
                    CreateAt = log.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task CreateLogProject(LogProject log)
        {
            _context.LogProjects.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
