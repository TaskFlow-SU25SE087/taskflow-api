using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
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

        public Task<int> CountBoard(Guid projectId)
        {
            return _context.Boards
                .CountAsync(b => b.ProjectId == projectId && b.IsActive);
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

        public async Task<Board?> GetBoardByTypeAsync(Guid projectId, BoardType type)
        {
            return await _context.Boards
                .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.Type == type && b.IsActive);
        }

        public async Task<List<Board>?> GetBoardsAfterOrderAsync(int order)
        {
            var listBoard = await _context.Boards
                .Where(b => b.Order > order && b.IsActive)
                .OrderBy(b => b.Order)
                .ToListAsync();
            return listBoard;
        }

        public Task<List<Board>> GetBoardsByIdsAsync(List<Guid> boardIds)
        {
            return _context.Boards
                .Where(b => boardIds.Contains(b.Id) && b.IsActive)
                .ToListAsync();
        }

        public async Task<Guid> GetIdBoardOrderFirtsAsync(Guid ProjectId)
        {
            var BoardId = await _context.Boards
                .Where(b => b.ProjectId == ProjectId && b.IsActive && b.Order ==  0)
                .OrderBy(b => b.Order)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();
            return BoardId;
        }

        public async Task<List<BoardResponse>> GetListBoardAsync(Guid ProjectID)
        {
            var boards = await _context.Boards
                .Where(b => b.ProjectId == ProjectID && b.IsActive)
                .OrderBy(b => b.Order)
                .Select(b => new BoardResponse
                {
                    Id = b.Id,
                    ProjectId = b.ProjectId,
                    Name = b.Name,
                    Description = b.Description,
                    Order = b.Order,
                    Type = b.Type
                })
                .ToListAsync();
            return boards;
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
