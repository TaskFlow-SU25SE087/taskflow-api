using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class LogProjectRepository : ILogProjectRepository
    {
        private readonly TaskFlowDbContext _context;

        public LogProjectRepository(TaskFlowDbContext context)
        {
            _context = context;
        }
    }
}
