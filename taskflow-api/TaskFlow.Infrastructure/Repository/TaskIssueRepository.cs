using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
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

        public async Task<List<IssueDetailResponse>> GetIssue(
            Guid projectId, IssueStatus? status = null, 
            TypeIssue? type = null, TaskPriority? priority = null)
        {
            var query = _context.Issues
                .Where(i => i.ProjectId == projectId && i.IsActive)
                .Include(i => i.TaskProject)
                .Include(i => i.CreatedByMember)
                    .ThenInclude(m => m.User)
                .Include(i => i.TaskAssignees)
                    .ThenInclude(ta => ta.ProjectMember)
                        .ThenInclude(pm => pm.User)
                 .AsQueryable();
            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (type.HasValue)
                query = query.Where(i => i.Type == type.Value);

            if (priority.HasValue)
                query = query.Where(i => i.Priority == priority.Value);

            var data = await query
                .Select(i => new IssueDetailResponse
                {
                    Id = i.Id,
                    TaskProjectID = i.TaskProjectId,
                    TitleTask = i.TaskProject!.Title,
                    PriorityTask = i.TaskProject.Priority,
                    CreatedBy = i.CreatedBy,
                    NameCreate = i.CreatedByMember!.User.FullName,
                    AvatarCreate = i.CreatedByMember.User.Avatar,
                    RoleCreate = i.CreatedByMember.Role,
                    Title = i.Title,
                    Description = i.Description,
                    Explanation = i.Explanation,
                    Example = i.Example,
                    Priority = i.Priority,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt,
                    Type = i.Type,
                    Status = i.Status,
                    IssueAttachmentUrls = i.IssueAttachmentUrlsList,
                    TaskAssignees = i.TaskAssignees.Select(ta => new TaskAssigneeResponseInIssue
                    {
                        ProjectMemberId = ta.ProjectMember.Id,
                        Executor = ta.ProjectMember.User.FullName,
                        Avatar = ta.ProjectMember.User.Avatar,
                        Role = ta.ProjectMember.Role
                    }).ToList()
                })
                .ToListAsync();

            return data;
        }
    }
}
