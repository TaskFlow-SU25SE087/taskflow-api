namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class ImportFileJobMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string usrFile { get; set; } = default!;
        public string JobId { get; set; } = Guid.NewGuid().ToString();
    }
}
