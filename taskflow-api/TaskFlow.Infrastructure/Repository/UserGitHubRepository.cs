using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class UserGitHubRepository : IUserGitHubRepository
    {
        private readonly TaskFlowDbContext _context;

        public UserGitHubRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetTokenByIdAsync(Guid? userGitHubTokenId)
        {
            var token = await _context.UserGitHubTokens
                .FirstOrDefaultAsync(x => x.Id == userGitHubTokenId);
            return token?.AccessToken ?? string.Empty;
        }

        public async Task<UserGitHubToken?> GetTokenByUserIdAsync(Guid userId)
        {
            return await _context.UserGitHubTokens
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task SaveTokenAsync(UserGitHubToken token)
        {
            _context.UserGitHubTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task Update(UserGitHubToken data)
        {
            _context.UserGitHubTokens.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
