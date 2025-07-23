using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace taskflow_api.TaskFlow.API.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
        
    }
}
