using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;


namespace taskflow_api.TaskFlow.Infrastructure.Repository
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

        public Task<Project?> GetProjectByIdAsync(Guid id)
        {
            var project = _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Boards)
                .Include(p => p.Sprints)
                .Include(p => p.TaskProject)
                .FirstOrDefaultAsync(p => p.Id == id);
            return project;
        }

        public async Task UpdateProject(Project data)
        {
            _context.Projects.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
