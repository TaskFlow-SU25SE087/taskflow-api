namespace taskflow_api.TaskFlow.Shared.Exceptions
{
    public class AppException : Exception
    {
        public ErrorDetail ErrorDetail { get; }
        public AppException(ErrorDetail errorDetail)
        {
            ErrorDetail = errorDetail;
        }
    }
}
