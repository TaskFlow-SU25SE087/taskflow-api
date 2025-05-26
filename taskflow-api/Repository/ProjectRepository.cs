using taskflow_api.Data;
using taskflow_api.Entity;

namespace taskflow_api.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TaskFlowDbContext _context;

        public ProjectRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateProjectAsync(string title)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title = title,
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project.Id;
        }
    }
}
