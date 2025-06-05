using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using System.Security.Cryptography;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;

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
        private readonly IRefreshTokenRepository _refeshTokenRepository;
        private readonly IMailService _mailService;
        private readonly IVerifyTokenRopository _verifyTokenRopository;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration configuration, IWebHostEnvironment env, 
            IHttpContextAccessor httpContextAccessor, IMapper mapper,
            IRefreshTokenRepository refeshTokenRepository, IMailService mailService,
            IVerifyTokenRopository verifyTokenRopository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _refeshTokenRepository = refeshTokenRepository;
            _mailService = mailService;
            _verifyTokenRopository = verifyTokenRopository;
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
            if (!user.IsPermanentlyBanned)
            {
                throw new AppException(ErrorCode.UserAlreadyBanned);
            }
            user.IsPermanentlyBanned = false;
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
                IsActive = user.IsPermanentlyBanned,
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
            if (user.IsPermanentlyBanned)
            {
                throw new AppException(ErrorCode.UserNotBanned);
            }
            user.IsPermanentlyBanned = true;
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
                IsActive = user.IsPermanentlyBanned,
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

        public async Task<TokenModel> Login(LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) throw new AppException(ErrorCode.InvalidEmail);
            if (user.IsPermanentlyBanned) throw new AppException(ErrorCode.AccountBanned);
            if (user.IsBanned) throw new AppException(ErrorCode.AccountBanned);

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid) throw new AppException(ErrorCode.InvalidPassword);
            var token = await GenerateToken(user);
            //var authClaims = new List<Claim>
            //{
            //    new Claim("ID", user.Id.ToString()),
            //    new Claim("Email", user.Email!),
            //    new Claim(ClaimTypes.Role, user.Role.ToString()),
            //    new Claim("Fullname", user.FullName!),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            //};

            //var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            //var token = new JwtSecurityToken(
            //    issuer: _configuration["Jwt:ValidIssuer"],
            //    audience: _configuration["Jwt:ValidAudience"],
            //    expires: DateTime.UtcNow.AddHours(1),
            //    claims: authClaims,
            //    signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha256)

            //);
            //var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return token!;
        }

        public async Task<TokenModel> RegisterAccount(RegisterAccountRequest model)
        {
            var existingGmail = await _userManager.FindByEmailAsync(model.Email);
            var existingUserName = await _userManager.FindByNameAsync(model.Email);

            //check Username
            if (existingUserName != null)
            {
                if (existingUserName.IsBanned)
                {
                    throw new AppException(ErrorCode.AccountBanned);
                }
                throw new AppException(ErrorCode.UsernameExists);
            }

            //check email exists
            if (existingGmail != null)
            {
                if (existingGmail.EmailConfirmed)
                {
                    throw new AppException(ErrorCode.EmailExists);
                }
                else
                {
                    await _userManager.DeleteAsync(existingGmail);
                    var verifyToken = await _verifyTokenRopository
                        .GetVerifyTokenByUserIdAndType(existingGmail.Id, VerifyTokenEnum.VerifyAccount);
                    if (verifyToken != null)
                    {
                        verifyToken.IsLocked = true;
                        await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
                    }
                }
                
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Username,
                Role = UserRole.User,
            };

            string avatarPath = string.Empty;
            if (model.Avatar != null)
            {
                 avatarPath = await ImageHelper.UploadImage(model.Avatar, _env.WebRootPath,
                     "Image/Avatars", Guid.NewGuid().ToString());
                user.Avatar = avatarPath;
            }

            var saveUser = await _userManager.CreateAsync(user, model.Password);
            
            if (saveUser.Succeeded)
            {
                string tokenVerify = GenerateRandom.GenerateRandomNumber();
                var verifyToken = new VerifyToken
                {
                    UserId = user.Id,
                    Token = tokenVerify,
                    Type = VerifyTokenEnum.VerifyAccount,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                };
                await _verifyTokenRopository.AddVerifyTokenAsync(verifyToken);
                await _mailService.VerifyAccount(model.Email, tokenVerify);
            }

            //error UserManager.CreateAsync
            if (!saveUser.Succeeded)
            {
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    ImageHelper.DeleteImage(avatarPath, _env.WebRootPath);
                }
                var errorMessages = string.Join("; ", saveUser.Errors.Select(e => e.Description));
                throw new AppException(new ErrorDetail(
                    1000,
                    errorMessages,
                    StatusCodes.Status400BadRequest
                    ));
            }
            var result = await Login(new LoginRequest
            {
                Username = model.Username,
                Password = model.Password
            });
            return result!;

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

        private async Task<TokenModel> GenerateToken(User user)
        {
            // Generate JWT token
            var authClaims = new List<Claim>
            {
                new Claim("ID", user.Id.ToString()),
                new Claim("Email", user.Email!),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Fullname", user.FullName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(15),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256)
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRandom.GenerateRandomToken();

            // Save refresh token to database
            var refreshTokenEntity = new RefeshToken
            {
                UserId = user.Id,
                JwtID = token.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssueAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Refresh token valid for 7 days
            };
             await _refeshTokenRepository.CreateRefreshTokenAsync(refreshTokenEntity);

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> VerifyAccount(string token)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var verifyToken = await _verifyTokenRopository.GetVerifyTokenByUserIdAndType(Guid.Parse(UserId!), VerifyTokenEnum.VerifyAccount);
            if (!verifyToken!.Token.Equals(token))
            {
                verifyToken.Attempts++;
                if (verifyToken.Attempts >= 6)
                {
                    verifyToken.IsLocked = true;
                    throw new AppException(ErrorCode.TooManyAttempts);
                }
                await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
                throw new AppException(ErrorCode.InvalidToken);
            }
            verifyToken.IsUsed = true;
            var user = await _userManager.FindByIdAsync(UserId!.ToString());
            user!.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<TokenModel> RenewToken(TokenModel model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(model.AccessToken);
            var jwtIdFromToken = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var stroredToken = await _refeshTokenRepository.GetRefreshTokenByToken(model.RefreshToken);
            if (stroredToken == null || stroredToken.IsUsed || stroredToken.IsRevoked || stroredToken.JwtID != jwtIdFromToken)
            {
                throw new AppException(ErrorCode.InvalidRefreshToken);
            }
            if (stroredToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new AppException(ErrorCode.RefreshTokenExpired);
            }
            // Mark the old refresh token as used
            stroredToken.IsRevoked = true;
            stroredToken.IsUsed = true;
            await _refeshTokenRepository.UpdateRefreshTokenAsync(stroredToken);

            //create new token
            var　user = await _userManager.FindByIdAsync(stroredToken.UserId.ToString());
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            var token = await GenerateToken(user);

            return token;
        }
    }
}
