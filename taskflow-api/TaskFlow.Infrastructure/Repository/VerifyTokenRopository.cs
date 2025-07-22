using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class VerifyTokenRopository : IVerifyTokenRopository
    {
        private readonly TaskFlowDbContext _context;

        public VerifyTokenRopository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddVerifyTokenAsync(VerifyToken data)
        {
            _context.VerifyTokens.Add(data);
            await _context.SaveChangesAsync();
        }

        public async Task<VerifyToken?> GetVerifyTokenAsync(string token)
        {
            var verifyToken = await _context.VerifyTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
            return verifyToken;
        }

        public Task<VerifyToken?> GetVerifyTokenByUserIdAndType(Guid id, VerifyTokenEnum Type)
        {
            var verifyToken = _context.VerifyTokens
                .FirstOrDefaultAsync(t => t.UserId == id && t.Type == Type && !t.IsUsed && !t.IsLocked);
            return verifyToken;
        }

        public async Task UpdateTokenAsync(VerifyToken data)
        {
            _context.VerifyTokens.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
