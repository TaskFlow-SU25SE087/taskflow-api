using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class LabelRepository : ILabelRepository
    {
        private readonly TaskFlowDbContext _context;

        public LabelRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddLabelAsync(Labels label)
        {
            await _context.Labels.AddAsync(label);
            await _context.SaveChangesAsync();
        }
    }
}
