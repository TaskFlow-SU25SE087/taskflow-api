using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class UserIntegrationRepository : IUserIntegrationRepository
    {
        private readonly TaskFlowDbContext _context;

        public UserIntegrationRepository(TaskFlowDbContext context)
        {
            _context = context;
        }
    }
}
