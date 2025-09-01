using Microsoft.AspNetCore.Identity;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IUserService
    {
        Task<TokenModel> RegisterAccount(RegisterAccountRequest model);
        Task<TokenModel> Login(LoginRequest model);
        Task<UserResponse> AddUserName(AddProfileUser model);
        Task<PagedResult<UserAdminResponse>> GetAllUser(int Page);
        Task<PagedResult<UserAdminResponse>> GetUsersByTerm(Guid termId, int page);
        Task<UserAdminResponse> BanUser(Guid userId);
        Task<UserAdminResponse> UnBanUser(Guid userId);
        Task<UserResponse> GetUserById(Guid userId);
        Task<UserResponse> UpdateUser(Guid userId, UpdateUserRequest model);
        Task<TokenModel> RenewToken(TokenModel model);
        Task<bool> VerifyAccount(string token);
        Task ImportEnrollmentsFromExcelAsync(ImportFileJobMessage file);
        Task SendMailAgain();
        Task ConfirmEmailAndSetPasswordAsync(ActivateAccountRequest request);
        Task ResetPassword(ResetPasswordRequest request);
        Task SendMailResetPassword(string EmailOrUsername);
        Task ImportFileExcelAsync(ImportUserFileRequest file);
        Task<PagedResult<ProcessingFile>> getListFileProcess(int page);
        Task ChangePassword(ChangePasswordRequest request);
    }
}
