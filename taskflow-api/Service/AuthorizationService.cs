using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using taskflow_api.Data;
using taskflow_api.Enums;

namespace taskflow_api.Service
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TaskFlowDbContext _context;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor, TaskFlowDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        public async Task<bool> AuthorizeAsync(Guid projectId, params ProjectRole[] allowedRoles)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null) return false;

            var userId = Guid.Parse(userIdClaim.Value);

            var projectMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.User.Id == userId);

            if (projectMember == null || !projectMember.IsActive) return false;

            return allowedRoles.Contains(projectMember.Role);
        }

    }
}
