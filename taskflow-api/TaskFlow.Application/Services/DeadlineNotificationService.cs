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
        private readonly IServiceProvider _serviceProvider;

        public DeadlineNotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var taskProjectRepository = scope.ServiceProvider.GetRequiredService<ITaskProjectRepository>();
                var taskAssigneeRepository = scope.ServiceProvider.GetRequiredService<ITaskAssigneeRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var now = DateTime.UtcNow;
                var tasks = await taskProjectRepository.GetAllActiveTasksAsync();
                foreach (var task in tasks)
                {
                    if (task.Deadline70Notified || task.Deadline <= task.CreatedAt)
                        continue;
                    var totalDuration = task.Deadline - task.CreatedAt;
                    var elapsed = now - task.CreatedAt;
                    if (elapsed.TotalSeconds / totalDuration.TotalSeconds >= 0.7)
                    {
                        var assignees = await taskAssigneeRepository.taskAssigneesAsync(task.Id);
                        foreach (var assignee in assignees)
                        {
                            if (assignee.ImplementerId.HasValue)
                            {
                                await notificationService.NotifyTaskUpdateAsync(
                                    assignee.ImplementerId.Value,
                                    task.ProjectId,
                                    task.Id,
                                    $"Task '{task.Title}' is approaching its deadline (70% of time elapsed). Please review and take necessary action."
                                );
                            }
                        }
                        task.Deadline70Notified = true;
                        await taskProjectRepository.UpdateTaskAsync(task);
                    }
                }
            }
        }
    }
}
