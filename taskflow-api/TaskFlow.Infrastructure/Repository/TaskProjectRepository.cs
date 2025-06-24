using Microsoft.EntityFrameworkCore;
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
                    Status = t.Board!.Name,
                    Deadline = t.Deadline,
                    AttachmentUrls = t.AttachmentUrl,
                    AttachmentUrlsList = t.CompletionAttachmentUrlsList,
                    SprintName = t.Sprint != null ? t.Sprint.Name : "No Sprint",
                    commnets = t.TaskComments
                        .Select(c => new CommnetResponse
                        {
                            Commenter = c.UserComment.User.FullName,
                            Avatar = c.UserComment.User.Avatar!,
                            Content = c.Content,
                            LastUpdate = c.LastUpdatedAt
                        }).ToList(),
                    TaskAssignees = (
                        from ta in _context.TaskAssignees
                        join pm in _context.ProjectMembers on ta.AssignerId equals pm.Id
                        join u in _context.Users on pm.UserId equals u.Id
                        where ta.RefId == t.Id && ta.Type == RefType.Task && ta.IsActive
                        select new TaskAssigneeResponse
                        {
                            Executor = u.FullName,
                            Avatar = u.Avatar,
                            Role = pm.Role
                        }
                    ).ToList(),
                    Tags = t.TaskTags
                        .Select(tt => new TaskTagResponse
                        {
                            Name = tt.Tag.Name,
                            Description = tt.Tag.Description,
                            Color = tt.Tag.Color
                        }).ToList()
                })
                .ToListAsync();
        }

        public async Task<TaskProject?> GetTaskByIdAsync(Guid id)
        {
            var task = await _context.TaskProjects
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            return task;
        }

        public async Task UpdateTaskAsync(TaskProject task)
        {
            _context.TaskProjects.Update(task);
            await _context.SaveChangesAsync();
        }
    }
}
