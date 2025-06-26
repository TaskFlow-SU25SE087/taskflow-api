using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TaskFlowAuthorizationService : ITaskFlowAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TaskFlowDbContext _context;

        public TaskFlowAuthorizationService(IHttpContextAccessor httpContextAccessor, TaskFlowDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Task<ProjectMember> AuthorizeAndGetMemberAsync(Guid projectId, params ProjectRole[] allowedRoles)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AuthorizeAsync(Guid projectId, params ProjectRole[] allowedRoles)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            //var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);;
            var userIdClaim = httpContext.User.FindFirst("ID");
            if (userIdClaim == null) return false;
            var userId = Guid.Parse(userIdClaim.Value);

            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine($"Project ID: {projectId}");
            var projectMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.User.Id == userId);

            if (projectMember == null || !projectMember.IsActive) return false;

            Console.WriteLine($"Project Member Role: {projectMember.Role}");
            return allowedRoles.Contains(projectMember.Role);
        }

    }
}
