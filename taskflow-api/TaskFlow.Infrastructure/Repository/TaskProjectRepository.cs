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
                .Select(t => new
                {
                    Task = t,
                    BoardOrder = t.Board.Order
                })
                .OrderBy(x => x.BoardOrder)
                .ThenBy(x => x.Task.Deadline)
                .ThenBy(x => x.Task.Priority)
                .Select(ts => new TaskProjectResponse
                {
                    Id = ts.Task.Id,
                    Title = ts.Task.Title,
                    Description = ts.Task.Description,
                    Priority = ts.Task.Priority,
                    CreatedAt = ts.Task.CreatedAt,
                    UpdatedAt = ts.Task.UpdatedAt,
                    Status = ts.Task.Board!.Name,
                    Deadline = ts.Task.Deadline,
                    AttachmentUrls = ts.Task.AttachmentUrls,
                    AttachmentUrlsList = ts.Task.AttachmentUrlsList,
                    SprintName = ts.Task.Sprint != null ? ts.Task.Sprint.Name : "No Sprint",
                    commnets = ts.Task.TaskComments
                        .Select(c => new CommnetResponse
                        {
                            Commenter = c.UserComment.User.FullName,
                            Avatar = c.UserComment.User.Avatar!,
                            Content = c.Content,
                            LastUpdate = c.LastUpdatedAt

                        }).ToList(),
                    TaskAssignees = ts.Task.TaskAssignees
                        .Where(ta => ta.Type == RefType.Task)
                        .Select(ta => new TaskAssigneeResponse
                        {
                            Executor = ta.ProjectMember.User.FullName,
                            Avatar = ta.ProjectMember.User.Avatar,
                            Role = ta.ProjectMember.Role
                        }).ToList(),
                    Tags = ts.Task.TaskTags.Select(tt => new TaskTagResponse
                    {
                        Name = tt.Tag.Name,
                        Description = tt.Tag.Description,
                        Color = tt.Tag.Color
                    }).ToList(),
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
