namespace taskflow_api.Model.Response
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = null!;
        public T Data { get; set; } = default!;

        //success response
        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T>
            {
                Code = 200,
                Message = "Success",
                Data = data
            };
        }

        //error response
        public static ApiResponse<T> Error(int code, string message)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = default!
            };
        }
    }
}
