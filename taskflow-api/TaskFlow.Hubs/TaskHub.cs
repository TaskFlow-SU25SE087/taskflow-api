using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace taskflow_api.TaskFlow.API.Hubs
{
    public class TaskHub : Hub
    {
        // Optional: group users by project
        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
        }
    }
}
