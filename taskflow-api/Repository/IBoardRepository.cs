using taskflow_api.Entity;

namespace taskflow_api.Repository
{
    public interface IBoardRepository
    {
        Task CreateBoardsAsync(Board data);
        Task CreateListBoardsAsync(List<Board> data);
    }
}
