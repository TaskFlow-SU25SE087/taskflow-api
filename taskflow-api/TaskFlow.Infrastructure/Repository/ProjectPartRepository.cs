using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class ProjectPartRepository : IProjectPartRepository
    {
        private readonly TaskFlowDbContext _context;

        public ProjectPartRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreatePartAsync(ProjectPart data)
        {
            await _context.ProjectParts.AddAsync(data);
            await _context.SaveChangesAsync();
        }

        public Task<List<ProjectPartResponse>> GetAllPartsByProjectIdAsync(Guid projectId)
        {
            return _context.ProjectParts
                .Where(x => x.ProjectId == projectId)
                .Select(x => new ProjectPartResponse
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    Name = x.Name,
                    ProgrammingLanguage = x.ProgrammingLanguage,
                    Framework = x.Framework,
                    RepoProvider = x.RepoProvider,
                    RepoUrl = x.RepoUrl,
                    OwnerId = x.UserGitHubToken.UserId,
                    OwnerName = x.UserGitHubToken.User.FullName,
                    AvatrarUrl = x.UserGitHubToken.User.Avatar
                })
                .ToListAsync();
        }

        public async Task<ProjectPart?> GetByRepoUrlAsync(string repoUrl)
        {
            return await _context.ProjectParts
                .FirstOrDefaultAsync(x => x.RepoUrl == repoUrl);

        }

        public async Task<ProjectPart?> GetPartByIdAsync(Guid partId)
        {
            return await _context.ProjectParts
                .FirstOrDefaultAsync(x => x.Id == partId);
        }

        public async Task UpdateAsync(ProjectPart part)
        {
            _context.ProjectParts.Update(part);
            await _context.SaveChangesAsync();
        }
    }
}
