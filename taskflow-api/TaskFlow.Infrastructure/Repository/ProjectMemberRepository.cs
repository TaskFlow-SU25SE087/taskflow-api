using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly TaskFlowDbContext _context;

        public ProjectMemberRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateProjectMemeberAsync(ProjectMember data)
        {
            data.Id = Guid.NewGuid();
            _context.ProjectMembers.Add(data);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectMember?> FindMemberInProject(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public Task<int> GetActiveMembersCount(Guid ProjectId)
        {
            return _context.ProjectMembers
                .CountAsync(pm => pm.ProjectId == ProjectId && pm.IsActive);
        }

        public Task<List<MemberResponse>> GetAllMembersInProjectAsync(Guid projectId)
        {
            return _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.IsActive)
                .Select(pm => new MemberResponse
                {
                    Id = pm.Id,
                    FullName = pm.User.FullName,
                    Avatar = pm.User.Avatar!,
                    Email = pm.User.Email!,
                    Role = pm.Role,
                })
                .ToListAsync();
        }

        public async Task<ProjectMemberResponse?> GetMeInProjectAsync(Guid projectId, Guid projectMemberId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.Id == projectMemberId && pm.IsActive)
                .Select(pm => new ProjectMemberResponse
                {
                    ProjectId = pm.ProjectId,
                    ProjectMemberId = pm.Id,
                    Role = pm.Role,
                })
                .FirstOrDefaultAsync();
        }

        public Task<int> GetProjectCountByUserIdAsync(Guid userId)
        {
            return _context.ProjectMembers
                .CountAsync(pm => pm.UserId == userId && pm.IsActive);
        }

        public async Task<bool> GetUserIsActiveInProjectAsync(Guid userId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive && pm.Project.IsActive)
                .AnyAsync();
        }

        public Task<bool> IsUserInProjectAsync(Guid projectId, Guid userId)
        {
            return _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.IsActive
                && pm.Project.Term.EndDate > DateTime.UtcNow && !pm.Project.Term.IsActive);
        }

        public async Task UpdateMember(ProjectMember data)
        {
            _context.ProjectMembers.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
