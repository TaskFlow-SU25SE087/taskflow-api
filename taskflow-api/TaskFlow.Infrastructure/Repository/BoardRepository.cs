using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class BoardRepository : IBoardRepository
    {
        private readonly TaskFlowDbContext _context;

        public BoardRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateBoardsAsync(Board data)
        {
            _context.Boards.Add(data);
            await _context.SaveChangesAsync();
        }

        public async Task CreateListBoardsAsync(List<Board> data)
        {
            foreach (var board in data)
            {
                _context.Boards.Add(board);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Board?> GetBoardByIdAsync(Guid id)
        {
            return await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        }

        public async Task<List<Board>?> GetBoardsAfterOrderAsync(int order)
        {
            var listBoard = await _context.Boards
                .Where(b => b.Order > order && b.IsActive)
                .OrderBy(b => b.Order)
                .ToListAsync();
            return listBoard;
        }

        public async Task<int> GetMaxOrder(Guid projectId)
        {
            var maxOrder = await _context.Boards
                .Where(b => b.ProjectId == projectId && b.IsActive)
                .MaxAsync(b => (int?)b.Order);
            return maxOrder ?? 0;
        }

        public async Task UpdateBoard(Board data)
        {
            _context.Boards.Update(data);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateListBoardsAsync(List<Board> data)
        {
            foreach (var board in data)
            {
                _context.Boards.Update(board);
            }
            await _context.SaveChangesAsync();
        }
    }
}
