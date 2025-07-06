using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class DeadlineNotificationService : BackgroundService
    {
        private readonly ITaskProjectRepository _taskProjectRepository;
        private readonly ITaskAssigneeRepository _taskAssigneeRepository;
        private readonly INotificationService _notificationService;

        public DeadlineNotificationService(
            ITaskProjectRepository taskProjectRepository,
            ITaskAssigneeRepository taskAssigneeRepository,
            INotificationService notificationService)
        {
            _taskProjectRepository = taskProjectRepository;
            _taskAssigneeRepository = taskAssigneeRepository;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndNotifyTasksAsync();
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Check every 10 minutes
            }
        }

        private async Task CheckAndNotifyTasksAsync()
        {
            var now = DateTime.UtcNow;
            var tasks = await _taskProjectRepository.GetAllActiveTasksAsync();
            foreach (var task in tasks)
            {
                if (task.Deadline70Notified || task.Deadline <= task.CreatedAt)
                    continue;
                var totalDuration = task.Deadline - task.CreatedAt;
                var elapsed = now - task.CreatedAt;
                if (elapsed.TotalSeconds / totalDuration.TotalSeconds >= 0.7)
                {
                    var assignees = await _taskAssigneeRepository.taskAssigneesAsync(task.Id);
                    foreach (var assignee in assignees)
                    {
                        if (assignee.ImplementerId.HasValue)
                        {
                            await _notificationService.NotifyTaskUpdateAsync(
                                assignee.ImplementerId.Value,
                                task.ProjectId,
                                task.Id,
                                $"Task '{task.Title}' is approaching its deadline (70% of time elapsed). Please review and take necessary action."
                            );
                        }
                    }
                    task.Deadline70Notified = true;
                    await _taskProjectRepository.UpdateTaskAsync(task);
                }
            }
        }
    }
}
