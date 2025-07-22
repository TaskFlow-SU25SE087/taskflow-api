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
                .Include(pm => pm.Project)
                .ThenInclude(p => p.Term)
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.User.Id == userId);

            if (projectMember == null || !projectMember.IsActive) throw new AppException(ErrorCode.Unauthorized);

            if (projectMember.User.Role == UserRole.User)
            {
                var term = projectMember.Project.Term;
                if (term == null || !term.IsActive || term.EndDate < DateTime.UtcNow)
                    throw new AppException(ErrorCode.TermExpired);
            }

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
            if (httpContext == null) throw new AppException(ErrorCode.Unauthorized);

            //var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);;
            var userIdClaim = httpContext.User.FindFirst("ID");
            if (userIdClaim == null) throw new AppException(ErrorCode.Unauthorized);
            var userId = Guid.Parse(userIdClaim.Value);

            var projectMember = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .ThenInclude(p => p.Term)
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.User.Id == userId);

            if (projectMember == null || !projectMember.IsActive) throw new AppException(ErrorCode.Unauthorized);

            if (projectMember.User.Role == UserRole.User)
            {
                var term = projectMember.Project.Term;
                if (term == null || !term.IsActive || term.EndDate < DateTime.UtcNow)
                    throw new AppException(ErrorCode.TermExpired);
            }

            return allowedRoles.Contains(projectMember.Role);
        }

        public async Task<Guid> GetCurrentUserIdAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) throw new AppException(ErrorCode.Unauthorized);
            var userIdClaim = httpContext.User.FindFirst("ID");
            if (userIdClaim == null) throw new AppException(ErrorCode.Unauthorized);
            return Guid.Parse(userIdClaim.Value);
        }
    }
}
