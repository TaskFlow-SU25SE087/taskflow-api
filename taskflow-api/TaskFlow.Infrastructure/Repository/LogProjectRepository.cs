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

        public async Task<List<ProjectLogResponse>> AllLogPrj(Guid projectId, Guid? nextLogId, int pageSize)
        {
            IQueryable<LogProject> query = _context.LogProjects
                .AsNoTracking()
                .Where(log => log.ProjectId == projectId);

            if (nextLogId.HasValue)
            {
                var nextLog = await _context.LogProjects
                    .Where(l => l.Id == nextLogId.Value)
                    .Select(l => new { l.CreatedAt, l.Id })
                    .FirstOrDefaultAsync();

                if (nextLog != null)
                {
                    query = query.Where(l =>
                        l.CreatedAt < nextLog.CreatedAt ||
                        (l.CreatedAt == nextLog.CreatedAt && l.Id < nextLog.Id)
                    );
                }
            }
            return await query
            .OrderByDescending(log => log.CreatedAt)
            .ThenByDescending(log => log.Id)
            .Take(pageSize)
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
                Board = log.Board == null ? null : new BoardIdLogResponse
                {
                    BoardId = log.Board.Id,
                    BoardName = log.Board.Name
                },
                Sprint = log.Sprint == null ? null : new SprintIdLogResponse
                {
                    SprintId = log.Sprint.Id,
                    SprintName = log.Sprint.Name
                },
                Task = log.TaskProject == null ? null : new TaskIdLogResponse
                {
                    TaskId = log.TaskProject.Id,
                    TaskName = log.TaskProject.Title
                }
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
