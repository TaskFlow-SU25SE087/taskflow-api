namespace taskflow_api.TaskFlow.Application.DTOs.Common
{
    public class SonarQubeSetting
    {
        public string HostUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string ScannerPath { get; set; } = string.Empty;
    }
}
