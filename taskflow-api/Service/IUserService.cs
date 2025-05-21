using Microsoft.AspNetCore.Identity;
using taskflow_api.Model.Request;
using taskflow_api.Model.Response;

namespace taskflow_api.Service
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAccount(RegisterAccountRequest model);
        Task<string> Login(LoginRequest model);
    }
}
