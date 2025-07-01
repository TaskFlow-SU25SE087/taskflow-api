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
    }
}
