using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

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
            data.IsActive = true;
            _context.ProjectMembers.Add(data);
            await _context.SaveChangesAsync();
        }
    }
}
