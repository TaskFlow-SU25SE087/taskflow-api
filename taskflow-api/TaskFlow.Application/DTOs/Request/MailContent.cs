namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class MailContent
    {
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
