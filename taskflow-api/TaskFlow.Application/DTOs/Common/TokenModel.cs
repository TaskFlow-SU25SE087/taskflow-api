namespace taskflow_api.TaskFlow.Application.DTOs.Common
{
    public class TokenModel
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
