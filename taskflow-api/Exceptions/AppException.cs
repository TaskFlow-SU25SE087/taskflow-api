namespace taskflow_api.Exceptions
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
