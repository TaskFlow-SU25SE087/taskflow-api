using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly TaskFlowDbContext _context;

        public RefreshTokenRepository(TaskFlowDbContext context)
        {
            _context = context;
        }
        public async Task CreateRefreshTokenAsync(RefeshToken model)
        {
            await _context.RefeshTokens.AddAsync(model);
            await _context.SaveChangesAsync();
        }
        public async Task<RefeshToken?> GetRefreshTokenByToken(string token)
        {
            return await _context.RefeshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }
    }
}
