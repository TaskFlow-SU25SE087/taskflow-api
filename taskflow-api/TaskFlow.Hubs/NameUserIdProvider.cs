using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace taskflow_api.TaskFlow.API.Hubs
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("ID")?.Value;
        }
    }
} 