using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Shared.Exceptions;

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

        public async Task<Guid> AuthorizeAndGetMemberAsync(Guid projectId, params ProjectRole[] allowedRoles)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) throw new AppException(ErrorCode.Unauthorized);

            //var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);;
            var userIdClaim = httpContext.User.FindFirst("ID");
            if (userIdClaim == null) throw new AppException(ErrorCode.Unauthorized);
            var userId = Guid.Parse(userIdClaim.Value);

            var projectMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.User.Id == userId);

            if (projectMember == null || !projectMember.IsActive) throw new AppException(ErrorCode.Unauthorized);

            bool check = allowedRoles.Contains(projectMember.Role);
            if (!check)
            {
                throw new AppException(ErrorCode.NoPermission);
            }
            return projectMember.Id;
        }

        public async Task<bool> AuthorizeAsync(Guid projectId, params ProjectRole[] allowedRoles)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            //var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);;
            var userIdClaim = httpContext.User.FindFirst("ID");
            if (userIdClaim == null) return false;
            var userId = Guid.Parse(userIdClaim.Value);

            var projectMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.User.Id == userId);

            if (projectMember == null || !projectMember.IsActive) return false;

            return allowedRoles.Contains(projectMember.Role);
        }

    }
}
