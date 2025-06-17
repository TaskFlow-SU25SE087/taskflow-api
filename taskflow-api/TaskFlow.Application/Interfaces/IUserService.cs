using Microsoft.AspNetCore.Identity;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IUserService
    {
        Task<TokenModel> RegisterAccount(RegisterAccountRequest model);
        Task<TokenModel> Login(LoginRequest model);
        Task<UserResponse> AddUserName(AddProfileUser model);
        Task<PagedResult<UserAdminResponse>> GetAllUser(int Page);
        Task<UserAdminResponse> BanUser(Guid userId);
        Task<UserAdminResponse> UnBanUser(Guid userId);
        Task<UserResponse> GetUserById(Guid userId);
        Task<UserResponse> UpdateUser(Guid userId, UpdateUserRequest model);
        Task<TokenModel> RenewToken(TokenModel model);
        Task<bool> VerifyAccount(string token);
        Task ImportEnrollmentsFromExcelAsync(ImportUserFileRequest file);
        Task SendMailAgain();
        Task ConfirmEmailAndSetPasswordAsync(ActivateAccountRequest request);
        Task ResetPassword(ResetPasswordRequest request);

    }
}
