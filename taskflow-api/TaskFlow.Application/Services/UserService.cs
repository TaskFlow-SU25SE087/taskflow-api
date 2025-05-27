using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration configuration, IWebHostEnvironment env, 
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<UserAdminResponse> BanUser(Guid userId)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            if (user.Role == UserRole.Admin)
            {
                throw new AppException(ErrorCode.CannotBanAdmin);
            }
            if (!user.IsActive)
            {
                throw new AppException(ErrorCode.UserAlreadyBanned);
            }
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new AppException(new ErrorDetail(
                    1000,
                    errorMessages,
                    StatusCodes.Status400BadRequest
                    ));
            }
            return new UserAdminResponse
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                Role = user.Role.ToString(),
            };
        }

        public async Task<UserAdminResponse> UnBanUser(Guid userId)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            if (user.Role == UserRole.Admin)
            {
                throw new AppException(ErrorCode.CannotBanAdmin);
            }
            if (user.IsActive)
            {
                throw new AppException(ErrorCode.UserNotBanned);
            }
            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new AppException(new ErrorDetail(
                    1000,
                    errorMessages,
                    StatusCodes.Status400BadRequest
                    ));
            }
            return new UserAdminResponse
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                Role = user.Role.ToString(),
            };
        }

        public async Task<PagedResult<UserAdminResponse>> GetAllUser(PagingParams pagingParams)
        {
            var users = _userManager.Users
                .Where(u => u.Role != UserRole.Admin);

            var usersResponse = _mapper.ProjectTo<UserAdminResponse>(users);

            pagingParams.PageSize = 5;
            var pageUser = await usersResponse.ToPagedListAsync(pagingParams);
            if (!pageUser.Items.Any())
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            return pageUser;
        }

        public async Task<string> Login(LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) throw new AppException(ErrorCode.InvalidEmail);
            if (!user.IsActive) throw new AppException(ErrorCode.AccountBanned);

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid) throw new AppException(ErrorCode.InvalidPassword);

            var authClaims = new List<Claim>
            {
                new Claim("ID", user.Id.ToString()),
                new Claim("Email", user.Email!),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Fullname", user.FullName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha256)

            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IdentityResult> RegisterAccount(RegisterAccountRequest model)
        {
            //check email exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new AppException(ErrorCode.EmailExists);
            }


            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                Role = UserRole.User,
                IsActive = true,
            };

            string avatarPath = string.Empty;
            if (model.Avatar != null)
            {
                avatarPath = await ImageHelper.UploadImage(model.Avatar, _env.WebRootPath,
                     "Image/Avatars", Guid.NewGuid().ToString());
                user.Avatar = avatarPath;
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            //error UserManager.CreateAsync
            if (!result.Succeeded)
            {
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    ImageHelper.DeleteImage(avatarPath, _env.WebRootPath);
                }
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new AppException(new ErrorDetail(
                    1000,
                    errorMessages,
                    StatusCodes.Status400BadRequest
                    ));
            }
            return result;

        }

        public async Task<UserResponse> GetUserById(Guid userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> UpdateUser(Guid userId, UpdateUserRequest model)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            if (UserId == null || UserId != userId.ToString())
            {
                throw new AppException(ErrorCode.Unauthorized);
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            user.FullName = model.FullName;
            if (model.Avatar != null)
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    // Delete old avatar image if it exists???
                    //ImageHelper.DeleteImage(user.Avatar, _env.WebRootPath);
                }
                user.Avatar = await ImageHelper.UploadImage(model.Avatar, _env.WebRootPath,
                     "Image/Avatars", Guid.NewGuid().ToString());
            }
            var saveImage = await _userManager.UpdateAsync(user);
            if (!saveImage.Succeeded)
            {
                throw new AppException(ErrorCode.ImageNotCanSave);
            }
            var result = _mapper.Map<UserResponse>(user);
            return result;
        }
    }
}
