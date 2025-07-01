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

        public NotificationService(
            IMailService mailService,
            IProjectMemberRepository projectMemberRepository,
            INotificationRepository notificationRepository,
            IHubContext<TaskHub> hubContext)
        {
            _mailService = mailService;
            _projectMemberRepository = projectMemberRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task NotifyTaskUpdateAsync(Guid userId, Guid projectId, Guid taskId, string message)
        {
            // Email notification
            var member = await _projectMemberRepository.FindMemberInProject(projectId, userId);
            if (member != null && member.User != null && !string.IsNullOrEmpty(member.User.Email))
            {
                var mailContent = new MailContent
                {
                    To = member.User.Email,
                    Subject = "Task Updated Notification",
                    Body = message
                };
                await _mailService.SendMailAsync(mailContent);
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
                notification.CreatedAt
            });
        }
    }
}
