using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class SprintMeetingLogsRepository : ISprintMeetingLogsRepository
    {
        private readonly TaskFlowDbContext _context;

        public SprintMeetingLogsRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateMetting(SprintMeetingLog data)
        {
            _context.SprintMeetingLogs.Add(data);
             await _context.SaveChangesAsync();
        }
    }
}
