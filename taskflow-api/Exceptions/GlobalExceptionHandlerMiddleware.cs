using taskflow_api.Enums;
using taskflow_api.Model.Common;

namespace taskflow_api.Exceptions
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            //error caught
            catch (AppException ex)
            {
                _logger.LogError(ex, "AppException occurred occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex.ErrorDetail);
            }
            //error not caught
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ErrorCode.UncategorizedException);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, ErrorDetail errorDetail)
        {
           context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorDetail.StatusCode;

            var response = ApiResponse<object>.Error(errorDetail.Code, errorDetail.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
