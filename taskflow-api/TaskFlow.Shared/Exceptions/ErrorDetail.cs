namespace taskflow_api.TaskFlow.Shared.Exceptions
{
    public record ErrorDetail(
        int Code,
        string Message,
        int StatusCode
        )
    {
    }
}
