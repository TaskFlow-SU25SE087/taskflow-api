using Microsoft.EntityFrameworkCore;
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
