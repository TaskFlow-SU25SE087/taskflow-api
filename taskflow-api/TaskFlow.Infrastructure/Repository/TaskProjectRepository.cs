using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TaskProjectRepository : ITaskProjectRepository
    {
        private readonly TaskFlowDbContext _context;

        public TaskProjectRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddTaskAsync(TaskProject task)
        {
             _context.TaskProjects.Add(task);
            await _context.SaveChangesAsync();
        }

        public Task<List<ListTaskProjectNotSprint>> GetAllTaskNotSprint(Guid projectId)
        {
            return _context.TaskProjects
                .Where(t => t.ProjectId == projectId && t.SprintId == null
                && t.SprintId == null && t.IsActive)
                .Select(t => new ListTaskProjectNotSprint
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Deadline = t.Deadline,
                    AttachmentUrl = t.AttachmentUrl
                })
                .ToListAsync();
        }

        public async Task<List<TaskProjectResponse>> GetAllTaskProjectAsync(Guid projectId)
        {
            return await _context.TaskProjects
                .Where(t => t.ProjectId == projectId && t.IsActive)
                .Select(t => new TaskProjectResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    BoardId = (Guid)t.BoardId!,
                    Status = t.Board!.Name,
                    Deadline = t.Deadline,
                    AttachmentUrl = t.AttachmentUrl,
                    CompletionAttachmentUrls = t.CompletionAttachmentUrlsList,
                    SprintId = t.SprintId ?? Guid.Empty,
                    SprintName = t.Sprint != null ? t.Sprint.Name : "No Sprint",
                    Commnets = t.TaskComments
                        .Select(c => new CommnetResponse
                        {
                            Commenter = c.UserComment.User.FullName,
                            Avatar = c.UserComment.User.Avatar!,
                            Content = c.Content,
                            AttachmentUrls = c.AttachmentUrlsList,
                            LastUpdate = c.LastUpdatedAt
                        }).ToList(),
                    TaskAssignees = (
                        from ta in _context.TaskAssignees
                        join pm in _context.ProjectMembers on ta.ImplementerId equals pm.Id
                        join u in _context.Users on pm.UserId equals u.Id
                        where ta.RefId == t.Id && ta.Type == RefType.Task && ta.IsActive
                        select new TaskAssigneeResponse
                        {
                            ProjectMemberId = pm.Id,
                            Executor = u.FullName,
                            Avatar = u.Avatar,
                            Role = pm.Role,
                        }
                    ).ToList(),
                    Tags = t.TaskTags
                        .Select(tt => new TaskTagResponse
                        {
                            Name = tt.Tag.Name,
                            Description = tt.Tag.Description,
                            Color = tt.Tag.Color
                        }).ToList(),
                    Issues = t.Issues
                        .Select(i => new IssueTaskResponse
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Description = i.Description,
                            Priority = i.Priority,
                            Type = i.Type,
                            Status = i.Status,
                            CreatedAt = i.CreatedAt,
                            UpdatedAt = i.UpdatedAt,
                            IssueAttachmentUrls = i.IssueAttachmentUrlsList,
                            Explanation = i.Explanation,
                            Example = i.Example
                        }).ToList()
                })
                .ToListAsync();
        }

        public Task<List<TaskProjectResponse>> GetListTaskBySprintIdAsync(Guid SprintID)
        {
            return _context.TaskProjects
                .Where(t => t.SprintId == SprintID && t.IsActive)
                .Select(t => new TaskProjectResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Deadline = t.Deadline,
                    AttachmentUrl = t.AttachmentUrl,
                    CompletionAttachmentUrls = t.CompletionAttachmentUrlsList,
                    SprintId = t.SprintId ?? Guid.Empty,
                    SprintName = t.Sprint != null ? t.Sprint.Name : "No Sprint",
                    BoardId = (Guid)t.BoardId!,
                    Status = t.Board!.Name,
                    Commnets = t.TaskComments
                        .Select(c => new CommnetResponse
                        {
                            Commenter = c.UserComment.User.FullName,
                            Avatar = c.UserComment.User.Avatar!,
                            Content = c.Content,
                            AttachmentUrls = c.AttachmentUrlsList,
                            LastUpdate = c.LastUpdatedAt
                        }).ToList(),
                    TaskAssignees = (
                        from ta in _context.TaskAssignees
                        join pm in _context.ProjectMembers on ta.ImplementerId equals pm.Id
                        join u in _context.Users on pm.UserId equals u.Id
                        where ta.RefId == t.Id && ta.Type == RefType.Task && ta.IsActive
                        select new TaskAssigneeResponse
                        {
                            ProjectMemberId = pm.Id,
                            Executor = u.FullName,
                            Avatar = u.Avatar,
                            Role = pm.Role,
                        }
                    ).ToList(),
                    Tags = t.TaskTags
                        .Select(tt => new TaskTagResponse
                        {
                            Name = tt.Tag.Name,
                            Description = tt.Tag.Description,
                            Color = tt.Tag.Color
                        }).ToList(),
                    Issues = t.Issues
                        .Select(i => new IssueTaskResponse
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Description = i.Description,
                            Priority = i.Priority,
                            Type = i.Type,
                            Status = i.Status,
                            CreatedAt = i.CreatedAt,
                            UpdatedAt = i.UpdatedAt,
                            IssueAttachmentUrls = i.IssueAttachmentUrlsList,
                            Explanation = i.Explanation,
                            Example = i.Example
                        }).ToList()
                })
                .ToListAsync();
        }

        public Task<List<TaskProject>> GetListTasksByIdsAsync(List<Guid> taskIds)
        {
            return _context.TaskProjects
                .Where(t => taskIds.Contains(t.Id) && t.IsActive)
                .ToListAsync();
        }

        public Task<List<TaskProject>> GetListTasksBySprintsIdsAsync(Guid SprintID)
        {
            return _context.TaskProjects
                .Where(t => t.SprintId == SprintID && t.IsActive)
                .ToListAsync();
        }

        public async Task<TaskProject?> GetTaskByIdAsync(Guid id)
        {
            var task = await _context.TaskProjects
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            return task;
        }

        public async Task UpdateListTaskAsync(List<TaskProject> task)
        {
            _context.TaskProjects.UpdateRange(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(TaskProject task)
        {
            _context.TaskProjects.Update(task);
            await _context.SaveChangesAsync();
        }
    public Task<List<TaskProject>> GetAllActiveTasksAsync()
        {
            return _context.TaskProjects
                .Where(t => t.IsActive)
                .ToListAsync();
        }
    }
}
