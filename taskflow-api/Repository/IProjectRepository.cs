namespace taskflow_api.Repository
{
    public interface IProjectRepository
    {
        Task<Guid> CreateProjectAsync(string title);
    }
}
