namespace taskflow_api.Exceptions
{
    public record ErrorDetail(
        int Code, 
        string Message, 
        int StatusCode
        )
    {
    }
}
