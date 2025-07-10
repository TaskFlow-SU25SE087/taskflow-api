namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ICodeScanService
    {
        Task ScanCommit(string extractPath, string projectKey);
    }
}
