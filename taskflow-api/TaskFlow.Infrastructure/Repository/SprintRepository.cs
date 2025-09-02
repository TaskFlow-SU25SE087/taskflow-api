using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
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

        public Task<bool> CheckSprintName(Guid projectId, string name)
        {
            return _context.Sprints
                .Where(s => s.ProjectId == projectId && s.Name == name && s.IsActive)
                .AnyAsync();
        }

        public async Task<bool> CheckSprintStartDate(Guid projectId)
        {
            return await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.IsActive 
                && s.Status.Equals(SprintStatus.InProgress))
                .AnyAsync();
        }

        public async Task CreateSprintAsync(Sprint sprint)
        {
            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
        }

        public Task<SprintResponse?> GetCurrentSprint(Guid projectId)
        {
            return _context.Sprints
                .Where(s => s.ProjectId == projectId && s.IsActive
                && s.Status.Equals(SprintStatus.InProgress))
                .Select(s => new SprintResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Sprint?> GetNextSprint(Guid projectId, DateTime afterDate)
        {
            return await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.IsActive 
                && s.Status == SprintStatus.NotStarted && s.StartDate >= afterDate)
                .OrderBy(s => s.EndDate)
                .FirstOrDefaultAsync();
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

        public async Task<List<TaskCompleteDTO>> GetTaskCompletes(Guid sprintId, Guid projectId)
        {
            var lastBoardId = await _context.Boards
                .Where(b => b.ProjectId == projectId && b.IsActive)
                .OrderByDescending(b => b.Order)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            return await _context.TaskProjects
                .Include(tp => tp.Board)
                .Where(tp => tp.SprintId == sprintId && tp.Board.Type == BoardType.Done && tp.IsActive)
                .Select(tp => new TaskCompleteDTO
                {
                    Id = tp.Id,
                    Title = tp.Title,
                    Description = tp.Description,
                    Priority = tp.Priority,
                })
                .ToListAsync();
        }

        public async Task<List<UnfinishedTaskDto>> GetUnFinishTasks(Guid sprintId, Guid projectId)
        {
            var lastBoardId = await _context.Boards
                .Where(b => b.ProjectId == projectId && b.IsActive)
                .OrderByDescending(b => b.Order)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            return await _context.TaskProjects
                .Include(tp => tp.Board)
                .Where(tp => tp.SprintId == sprintId && tp.Board!.Type != BoardType.Done && tp.IsActive)
                .Select(tp => new UnfinishedTaskDto
                {
                    Id = tp.Id,
                    Title = tp.Title,
                    Description = tp.Description,
                    Priority = tp.Priority,
                })
                .ToListAsync();
        }

        public async Task<List<SprintTaskWithBoardInfo>> GetSprintTasksWithBoardInfo(Guid sprintId, Guid projectId)
        {
            return await _context.TaskProjects
                .Where(tp => tp.SprintId == sprintId && tp.IsActive)
                .Select(tp => new SprintTaskWithBoardInfo
                {
                    Id = tp.Id,
                    Title = tp.Title,
                    Description = tp.Description,
                    Priority = tp.Priority,
                    Deadline = tp.Deadline,
                    BoardId = tp.BoardId,
                    BoardName = tp.Board != null ? tp.Board.Name : "No Board",
                    BoardType = tp.Board != null ? tp.Board.Type : BoardType.Custom,
                    Assignees = tp.TaskAssignees
                        .Where(ta => ta.IsActive)
                        .Select(ta => ta.ProjectMember.User.FullName)
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task UpdateSprintAsync(Sprint sprint)
        {
            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSprintAsync(Guid sprintId)
        {
            var sprint = await _context.Sprints
                .Where(s => s.Id == sprintId && s.IsActive)
                .FirstOrDefaultAsync();

            if (sprint != null)
            {
                sprint.IsActive = false;
                _context.Sprints.Update(sprint);
                await _context.SaveChangesAsync();
            }
        }
    }
}
