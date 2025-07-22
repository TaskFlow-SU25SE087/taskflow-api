using System;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using taskflow_api.TaskFlow.API.Hubs;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMailService _mailService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<TaskHub> _hubContext;
        private readonly IProjectRepository _projectRepository;

        public NotificationService(
            IMailService mailService,
            IProjectMemberRepository projectMemberRepository,
            INotificationRepository notificationRepository,
            IHubContext<TaskHub> hubContext,
            IProjectRepository projectRepository)
        {
            _mailService = mailService;
            _projectMemberRepository = projectMemberRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _projectRepository = projectRepository;
        }

        public async Task NotifyTaskUpdateAsync(Guid userId, Guid projectId, Guid taskId, string message)
        {
            // Email notification
            var member = await _projectMemberRepository.FindMemberInProject(projectId, userId);
            if (member != null && member.User != null && !string.IsNullOrEmpty(member.User.Email))
            {
                await _mailService.SendTaskUpdateEmailAsync(
                    member.User.Email,
                    member.User.FullName ?? member.User.UserName ?? "User",
                    "Task Update",
                    message
                );
            }

            // In-app notification (database)
            var notification = new Notification
            {
                UserId = userId,
                ProjectId = projectId,
                TaskId = taskId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.AddNotificationAsync(notification);

            // SignalR notification (send to user)
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.UserId,
                notification.ProjectId,
                notification.TaskId,
                notification.Message,
                notification.IsRead,
                notification.CreatedAt,
                Type = "TaskUpdate"
            });
        }
    public async Task NotifyProjectMemberChangeAsync(Guid projectId, string message)
        {
            var members = await _projectMemberRepository.GetAllMembersInProjectAsync(projectId);
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            var projectName = project?.Title ?? "Project";
            foreach (var member in members)
            {
                // Email notification
                if (!string.IsNullOrEmpty(member.Email))
                {
                    await _mailService.SendProjectMemberChangeEmailAsync(
                        member.Email,
                        !string.IsNullOrEmpty(member.FullName) ? member.FullName : member.Email,
                        projectName,
                        message
                    );
                }

                // In-app notification (database)
                var notification = new Notification
                {
                    UserId = member.Id,
                    ProjectId = projectId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddNotificationAsync(notification);

                // SignalR notification (send to user)
                await _hubContext.Clients.User(member.Id.ToString()).SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.UserId,
                    notification.ProjectId,
                    notification.Message,
                    notification.IsRead,
                    notification.CreatedAt,
                    Type = "ProjectMemberChange"
                });
            }
        }
    public async Task NotifyTaskBoardChangeAsync(
            Guid projectId,
            Guid taskId,
            string oldBoardName,
            string newBoardName,
            List<Guid> userIds)
        {
            string message = $"Task has moved from board '{oldBoardName}' to '{newBoardName}'.";
            foreach (var userId in userIds)
            {
                // Email notification
                var member = await _projectMemberRepository.FindMemberInProject(projectId, userId);
                if (member != null && member.User != null && !string.IsNullOrEmpty(member.User.Email))
                {
                    await _mailService.SendTaskUpdateEmailAsync(
                        member.User.Email,
                        member.User.FullName ?? member.User.UserName ?? "User",
                        "Task Board Changed",
                        message
                    );
                }

                // In-app notification (database)
                var notification = new Notification
                {
                    UserId = userId,
                    ProjectId = projectId,
                    TaskId = taskId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddNotificationAsync(notification);

                // SignalR notification (send to user)
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.UserId,
                    notification.ProjectId,
                    notification.TaskId,
                    notification.Message,
                    notification.IsRead,
                    notification.CreatedAt,
                    Type = "TaskBoardChange"
                });
            }
        }
    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId);
        }
    }
}
