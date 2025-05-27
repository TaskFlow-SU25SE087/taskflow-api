using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IBoardRepository
    {
        Task CreateBoardsAsync(Board data);
        Task CreateListBoardsAsync(List<Board> data);
        Task<int> GetMaxOrder(Guid ProjectID);
    }
}
