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

        public async Task<Guid> CreateProjectAsync(Project data)
        {
            await _context.Projects.AddAsync(data);
            await _context.SaveChangesAsync();
            return data.Id;
        }

        public async Task<List<ProjectsResponse>> GetListProjectResponseByUserAsync(Guid userId)
        {
            return await _context.Projects
                .Where(p => p.Members.Any(m => m.UserId == userId && m.IsActive && m.Project.IsActive))
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

        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
                var project = await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.ProjectParts)
            .Include(p => p.Boards)
                .ThenInclude(b => b.TaskProject)
                    .ThenInclude(t => t.TaskComments)
            .Include(p => p.Boards)
                .ThenInclude(b => b.TaskProject)
                    .ThenInclude(t => t.TaskTags)
                        .ThenInclude(tl => tl.Tag)
            .Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (project != null)
            {
                foreach (var board in project.Boards)
                {
                    board.TaskProject = board.TaskProject
                        .Where(tp => tp.IsActive)
                        .ToList();

                    foreach (var task in board.TaskProject)
                    {
                        task.TaskTags = task.TaskTags
                            .Where(tt => tt.Tag != null)
                            .ToList();

                        task.TaskComments = task.TaskComments
                            .Where(c => !string.IsNullOrWhiteSpace(c.Content))
                            .ToList();
                    }
                }
            }

            return project;
        }

        public IQueryable<Project> GetProjectsByUserIdAsync(Guid userId)
        {
            return _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Boards)
                .Include(p => p.Sprints)
                .Include(p => p.TaskProject)
                .Where(p => p.Members.Any(m => m.UserId == userId && m.IsActive && m.Project.IsActive))
                .OrderByDescending(p => p.LastUpdate);
        }

        public async Task UpdateProject(Project data)
        {
            _context.Projects.Update(data);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return false;
            }

            // Set project as inactive instead of hard delete
            project.IsActive = false;
            project.LastUpdate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProjectsResponse>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Where(p => p.IsActive)
                .Select(p => new ProjectsResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    LastUpdate = p.LastUpdate,
                    Role = null // No specific role for admin view
                })
                .OrderByDescending(p => p.LastUpdate)
                .ToListAsync();
        }
    }
}
