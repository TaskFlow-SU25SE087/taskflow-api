namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProjectRepository
    {
        Task<Guid> CreateProjectAsync(string title);
    }
}
