using System;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using taskflow_api.TaskFlow.API.Hubs;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMailService _mailService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<TaskHub> _hubContext;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IMailService mailService,
            IProjectMemberRepository projectMemberRepository,
            INotificationRepository notificationRepository,
            IHubContext<TaskHub> hubContext,
            IProjectRepository projectRepository,
            ILogger<NotificationService> logger)
        {
            _mailService = mailService;
            _projectMemberRepository = projectMemberRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task NotifyTaskUpdateAsync(Guid userId, Guid projectId, Guid taskId, string message)
        {
            // Email notification
            var member = await _projectMemberRepository.FindMemberInProject(projectId, userId);
            if (member != null && member.User != null && !string.IsNullOrEmpty(member.User.Email))
            {
                _logger.LogInformation("Sending task update email to {Email} for user {UserId}", member.User.Email, userId);
                await _mailService.SendTaskUpdateEmailAsync(
                    member.User.Email,
                    member.User.FullName ?? member.User.UserName ?? "User",
                    "Task Update",
                    message
                );
            }
            else
            {
                _logger.LogWarning("Could not send email notification for user {UserId}. Member: {MemberNull}, User: {UserNull}, Email: {Email}", 
                    userId, member == null, member?.User == null, member?.User?.Email);
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
                    _logger.LogInformation("Sending project member change email to {Email} for member {MemberId}", member.Email, member.Id);
                    await _mailService.SendProjectMemberChangeEmailAsync(
                        member.Email,
                        !string.IsNullOrEmpty(member.FullName) ? member.FullName : member.Email,
                        projectName,
                        message
                    );
                }
                else
                {
                    _logger.LogWarning("Could not send project member change email for member {MemberId}. Email is null or empty.", member.Id);
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

        public async Task NotifyTaskAssignmentAsync(Guid userId, Guid projectId, Guid taskId, string taskTitle, string assignerName)
        {
            string message = $"You have been assigned to task '{taskTitle}' by {assignerName}.";
            
            // Email notification
            var member = await _projectMemberRepository.FindMemberInProject(projectId, userId);
            if (member != null && member.User != null && !string.IsNullOrEmpty(member.User.Email))
            {
                _logger.LogInformation("Sending task assignment email to {Email} for user {UserId}", member.User.Email, userId);
                await _mailService.SendTaskUpdateEmailAsync(
                    member.User.Email,
                    member.User.FullName ?? member.User.UserName ?? "User",
                    "Task Assignment",
                    message
                );
            }
            else
            {
                _logger.LogWarning("Could not send task assignment email for user {UserId}. Member: {MemberNull}, User: {UserNull}, Email: {Email}", 
                    userId, member == null, member?.User == null, member?.User?.Email);
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
                Type = "TaskAssignment"
            });
        }

    public async Task NotifyTaskBoardChangeAsync(
            Guid projectId,
            Guid taskId,
            string oldBoardName,
            string newBoardName,
            List<Guid> projectMemberIds)
        {
            string message = $"Task has moved from board '{oldBoardName}' to '{newBoardName}'.";
            foreach (var projectMemberId in projectMemberIds)
            {
                // Email notification
                var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
                if (member != null && member.User != null && !string.IsNullOrEmpty(member.User.Email))
                {
                    _logger.LogInformation("Sending task board change email to {Email} for project member {ProjectMemberId}", member.User.Email, projectMemberId);
                    await _mailService.SendTaskUpdateEmailAsync(
                        member.User.Email,
                        member.User.FullName ?? member.User.UserName ?? "User",
                        "Task Board Changed",
                        message
                    );
                }
                else
                {
                    _logger.LogWarning("Could not send task board change email for project member {ProjectMemberId}. Member: {MemberNull}, User: {UserNull}, Email: {Email}", 
                        projectMemberId, member == null, member?.User == null, member?.User?.Email);
                }

                // In-app notification (database)
                if (member != null)
                {
                    var notification = new Notification
                    {
                        UserId = member.UserId,
                        ProjectId = projectId,
                        TaskId = taskId,
                        Message = message,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddNotificationAsync(notification);

                    // SignalR notification (send to user)
                    await _hubContext.Clients.User(member.UserId.ToString()).SendAsync("ReceiveNotification", new
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
        }
    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task DeleteAllReadAsync(Guid userId)
        {
            await _notificationRepository.DeleteAllReadAsync(userId);
        }
    }
}
