namespace taskflow_api.TaskFlow.Application.DTOs.Common
{
    public class RabbitMQSetting
    {
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ScanCommitQueue { get; set; } = string.Empty;
    }
}
