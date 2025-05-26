using Microsoft.EntityFrameworkCore;
using taskflow_api.Data;
using taskflow_api.Entity;

namespace taskflow_api.Repository
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
            int Order = _context.Boards
                .Where(b => b.ProjectId == data.ProjectId
                && b.IsActive)
                .Select(b => b.Order)
                .DefaultIfEmpty(0)
                .Max() + 1;
            var board = new Board
            {
                Id = Guid.NewGuid(),
                Name = data.Name,
                Description = data.Description,
                ProjectId = data.ProjectId,
                IsActive = data.IsActive,
                Order = Order
            };
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
        }

        public async Task CreateListBoardsAsync(List<Board> data)
        {
            var projectId = data.First().ProjectId;
            var orders = await _context.Boards
                .Where(b => b.ProjectId == projectId && b.IsActive)
                .Select(b => b.Order)
                .ToListAsync();
            int Order = (orders.Count > 0 ? orders.Max() : 0) + 1;
            foreach (var board in data)
            {
                board.Id = Guid.NewGuid();
                board.Name = board.Name;
                board.Description = board.Description;
                board.ProjectId = board.ProjectId;
                board.IsActive = true;
                board.Order = Order++;
                _context.Boards.Add(board);
            }
            await _context.SaveChangesAsync();
        }
    }
}
