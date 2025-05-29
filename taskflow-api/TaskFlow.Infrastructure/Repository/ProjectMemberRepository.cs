using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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

        public async Task UpdateMember(ProjectMember data)
        {
            _context.ProjectMembers.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
