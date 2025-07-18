namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class SonarIssueResponse
    {
        public string Key { get; set; }
        public string Rule { get; set; }
        public string Severity { get; set; }
        public string Component { get; set; }
        public int Line { get; set; }
        public string Message { get; set; }
    }
}
