using System;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class NotificationService : INotificationService
    {
        public async Task NotifyTaskUpdateAsync(Guid userId, Guid taskId, string message)
        {
            // Simulate sending a notification (e.g., log, email, push, etc.)
            // For now, just log to console (replace with real implementation as needed)
            await Task.Run(() =>
            {
                Console.WriteLine($"[Notification] To User: {userId} | Task: {taskId} | Message: {message}");
            });
        }
    }
}
