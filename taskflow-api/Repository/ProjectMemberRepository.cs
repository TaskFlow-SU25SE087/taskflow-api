using taskflow_api.Data;
using taskflow_api.Entity;

namespace taskflow_api.Repository
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
            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                UserId = data.UserId,
                ProjectId = data.ProjectId,
                Role = data.Role,
                IsActive = true
            };
            _context.ProjectMembers.Add(projectMember);
            await _context.SaveChangesAsync();
        }
    }
}
