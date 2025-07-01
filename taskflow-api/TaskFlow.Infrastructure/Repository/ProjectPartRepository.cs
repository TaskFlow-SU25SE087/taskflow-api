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
    }
}
