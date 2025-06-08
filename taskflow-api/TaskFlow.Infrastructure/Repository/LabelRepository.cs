using Microsoft.EntityFrameworkCore;
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

        public async Task DeleteLabelAsync(Guid labelId)
        {
            var labelDelete = await _context.Labels.FindAsync(labelId);
            _context.Labels.Remove(labelDelete!);
                await _context.SaveChangesAsync();
        }

        public async Task<Labels?> GetLabelByIdAsync(Guid labelId)
        {
            return await _context.Labels
                    .FirstOrDefaultAsync(x => x.Id == labelId);
        }

        public async Task UpdateLabelAsync(Labels label)
        {
            _context.Labels.Update(label);
            await _context.SaveChangesAsync();
        }
    }
}
