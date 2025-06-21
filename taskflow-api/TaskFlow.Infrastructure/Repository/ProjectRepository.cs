using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
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

        public async Task<Guid> CreateProjectAsync(string title, string description, Guid OwnerId)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title  = title,
                Description = description,
                OwnerId = OwnerId,
                CreatedAt = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project.Id;
        }

        public async Task<List<ProjectsResponse>> GetListProjectResponseByUserAsync(Guid userId)
        {
            return await _context.Projects
                .Where(p => p.Members.Any(m => m.UserId == userId && m.IsActive))
                .Select(p => new ProjectsResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    LastUpdate = p.LastUpdate,
                    Role = p.Members
                        .Where(m => m.UserId == userId && m.IsActive)
                        .Select(m => m.Role)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public Task<Project?> GetProjectByIdAsync(Guid id)
        {
                var project = _context.Projects
            .Include(p => p.Members)
            .Include(p => p.Boards)
                .ThenInclude(b => b.TaskProject)
                    .ThenInclude(t => t.TaskComments)
            .Include(p => p.Boards)
                .ThenInclude(b => b.TaskProject)
                    .ThenInclude(t => t.TaskTags)
                        .ThenInclude(tl => tl.Tag)
            .Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == id);

                return project;
        }

        public IQueryable<Project> GetProjectsByUserIdAsync(Guid userId)
        {
            return _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Boards)
                .Include(p => p.Sprints)
                .Include(p => p.TaskProject)
                .Where(p => p.Members.Any(m => m.UserId == userId && m.IsActive))
                .OrderByDescending(p => p.LastUpdate);
        }

        public async Task UpdateProject(Project data)
        {
            _context.Projects.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
