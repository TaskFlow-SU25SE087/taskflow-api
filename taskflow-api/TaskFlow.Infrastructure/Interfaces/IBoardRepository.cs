using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IBoardRepository
    {
        Task CreateBoardsAsync(Board data);
        Task CreateListBoardsAsync(List<Board> data);
        Task<int> GetMaxOrder(Guid ProjectID);
        Task<Board?> GetBoardByIdAsync(Guid id);
        Task UpdateBoard(Board data);
        Task<List<Board>?> GetBoardsAfterOrderAsync(int order);
        Task UpdateListBoardsAsync(List<Board> data);
        Task<Guid> GetIdBoardOrderFirtsAsync(Guid ProjectId);
        Task<List<BoardResponse>> GetListBoardAsync(Guid ProjectID);
        Task<List<Board>> GetBoardsByIdsAsync(List<Guid> boardIds);
        Task<int> CountBoard(Guid projectId);
    }
}
