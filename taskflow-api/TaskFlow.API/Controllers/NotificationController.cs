using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.Interfaces;
using System.Security.Claims;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ITaskFlowAuthorizationService _authorizationService;

        public NotificationController(INotificationService notificationService, ITaskFlowAuthorizationService authorizationService)
        {
            _notificationService = notificationService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            try
            {
                var userId = await _authorizationService.GetCurrentUserIdAsync();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }
    }
}
