namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class CommitScanResult
    {
        public bool Success { get; set; }
        public string OutputLog { get; set; } = string.Empty;
        public string ErrorLog { get; set; } = string.Empty;
        public string ProjectKey { get; set; } = string.Empty;
    }
}
