﻿using AutoMapper;
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
using OfficeOpenXml;
using CloudinaryDotNet.Core;
using CloudinaryDotNet;
using Azure.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IRefreshTokenRepository _refeshTokenRepository;
        private readonly IMailService _mailService;
        private readonly IVerifyTokenRopository _verifyTokenRopository;
        private readonly IFileService _fileService;
        private readonly IMemoryCache _cache;
        private readonly ITermRepository _termRepository;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration configuration, IFileService fileService, 
            IHttpContextAccessor httpContextAccessor, IMapper mapper,
            IRefreshTokenRepository refeshTokenRepository, IMailService mailService,
            IVerifyTokenRopository verifyTokenRopository, IMemoryCache cache, ITermRepository termRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _refeshTokenRepository = refeshTokenRepository;
            _mailService = mailService;
            _verifyTokenRopository = verifyTokenRopository;
            _fileService = fileService;
            _cache = cache;
            _termRepository = termRepository;
        }

        public async Task<UserAdminResponse> BanUser(Guid userId)
        {
            //find user by id
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }

            //cannot ban admin
            if (user.Role == UserRole.Admin)
            {
                throw new AppException(ErrorCode.CannotBanAdmin);
            }

            //set IActive to false
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
            //find user by id
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            //cannot Unban admin
            if (user.Role == UserRole.Admin)
            {
                throw new AppException(ErrorCode.CannotBanAdmin);
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

        public async Task<PagedResult<UserAdminResponse>> GetAllUser(int Page)
        {
            var pagingParams = new PagingParams
            {
                PageNumber = Page,
                PageSize = 5
            };

            var usersQuery = _userManager.Users
                .AsNoTracking()
                .Where(u => u.Role != UserRole.Admin);

            var totalItems = await usersQuery.CountAsync();
            var items = await _mapper.ProjectTo<UserAdminResponse>(usersQuery)
                .OrderByDescending(u => u.IsActive)
                .ThenByDescending(u => u.TermYear)
                .ThenByDescending(u =>
                                    u.TermSeason == "Fall" ? 3 :
                                    u.TermSeason == "Summer" ? 2 :
                                    u.TermSeason == "Spring" ? 1 : 4)
                .ThenBy(u => u.FullName)
                .Skip(pagingParams.Skip)
                .Take(pagingParams.PageSize)
                .ToListAsync();

            if (!items.Any())
            {
                throw new AppException(ErrorCode.NoUserFound);
            }

            var result = new PagedResult<UserAdminResponse>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pagingParams.PageSize),
                PageNumber = pagingParams.PageNumber,
                PageSize = pagingParams.PageSize
            };
            return result;
        }

        public async Task<TokenModel> Login(LoginRequest model)
        {
            var user = await _userManager.Users
                .Include(u => u.Term)
                .FirstOrDefaultAsync(u => u.UserName == model.Username);
            if (user == null) throw new AppException(ErrorCode.InvalidPasswordOrUserName);
            if (user.LockoutEnabled) throw new AppException(ErrorCode.Unauthorized);
            if (!user.IsActive) throw new AppException(ErrorCode.AccountBanned);
            if (user.Role.Equals(UserRole.User))
            {
                if (user.Term.EndDate < DateTime.UtcNow && !user.Term.IsActive)
                throw new AppException(ErrorCode.AccountExpired);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid) throw new AppException(ErrorCode.InvalidPasswordOrUserName);
            var token = await GenerateToken(user);
            return token!;
        }

        public async Task<TokenModel> RegisterAccount(RegisterAccountRequest model)
        {
            var existingGmail = await _userManager.FindByEmailAsync(model.Email);

            //check email exists
            if (existingGmail != null)
            {
                if (existingGmail.EmailConfirmed)
                {
                    throw new AppException(ErrorCode.EmailExists);
                }
                else
                {
                    var verifyToken = await _verifyTokenRopository
                        .GetVerifyTokenByUserIdAndType(existingGmail.Id, VerifyTokenEnum.VerifyAccount);
                    if (verifyToken != null)
                    {
                        verifyToken.IsLocked = true;
                        await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
                    }
                    await _userManager.DeleteAsync(existingGmail);
                }
                
            }
            var baseAvatarUrl = _configuration["CloudinarySettings:BaseAvatarUrl"];
            var avatarPath = $"{baseAvatarUrl}/avatar/default.jpg";
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                Role = UserRole.User,
                Avatar = avatarPath,
                IsActive = true,
            };

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
                var errorMessages = string.Join("; ", saveUser.Errors.Select(e => e.Description));
                throw new AppException(new ErrorDetail(
                    1000,
                    errorMessages,
                    StatusCodes.Status400BadRequest
                    ));
            }
            var result = await Login(new LoginRequest
            {
                Username = model.Email,
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
            user.PhoneNumber = model.PhoneNumber;
            user.FullName = model.FullName;
            if (model.Avatar != null)
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {

                }

                var avatarPath = await _fileService.UploadPictureAsync(model.Avatar);
                if (avatarPath == null)
                {
                    avatarPath = user.Avatar; // Fallback to existing avatar if upload fails
                }
                user.Avatar = avatarPath;
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
                new Claim("Avatar", user.Avatar ?? string.Empty),
                new Claim("TermId", user.TermId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                expires: DateTime.UtcNow.AddDays(2),
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
            var email = httpContext?.User.FindFirst("email")?.Value;
            //check email exists
            bool isEmailExists = await _userManager.Users.AnyAsync(u => u.Email == email && u.EmailConfirmed);
            if (isEmailExists)
            {
                throw new AppException(ErrorCode.EmailAlreadyVerified);
            }

            var verifyToken = await _verifyTokenRopository.GetVerifyTokenByUserIdAndType(Guid.Parse(UserId!), VerifyTokenEnum.VerifyAccount);
            if (!verifyToken!.Token.Equals(token))
            {
                verifyToken.Attempts++;
                if (verifyToken.Attempts > 5)
                {
                    verifyToken.IsLocked = true;
                    throw new AppException(ErrorCode.TooManyAttempts);
                }
                await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
                throw new AppException(ErrorCode.InvalidToken);
            }
            verifyToken.IsUsed = true;
            await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
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
            if (!user.IsActive)
            {
                throw new AppException(ErrorCode.AccountBanned);
            }
            if (!user.Role.Equals(UserRole.User) && user.Term.EndDate < DateTime.UtcNow && !user.Term.IsActive)
            {
                throw new AppException(ErrorCode.AccountExpired);
            }
            var token = await GenerateToken(user);

            return token;
        }

        public async Task SendMailAgain()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var email = httpContext?.User.FindFirst("email")?.Value;
            var OldToken = await _verifyTokenRopository
                .GetVerifyTokenByUserIdAndType(Guid.Parse(UserId!), VerifyTokenEnum.VerifyAccount);

            //Clocked token 
            OldToken!.IsLocked = true;
            await _verifyTokenRopository.UpdateTokenAsync(OldToken);

            var newToken = GenerateRandom.GenerateRandomNumber();
            await _verifyTokenRopository.AddVerifyTokenAsync(new VerifyToken
            {
                UserId = Guid.Parse(UserId!),
                Token = newToken,
                Type = VerifyTokenEnum.VerifyAccount,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            });
            //send mail again
            await _mailService.VerifyAccount(email!, newToken);

        }

        public async Task<UserResponse> AddUserName(AddProfileUser model)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var user = await _userManager.FindByIdAsync(UserId!);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            if (!user.EmailConfirmed)
            {
                throw new AppException(ErrorCode.EmailNotConfirmed);
            }
            if (!user.Email.Equals(user.UserName))
            {
                throw new AppException(ErrorCode.UsernameAlreadyAdded);
            }
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                throw new AppException(ErrorCode.UsernameAlreadyExists);
            }
            if (model.Avatar != null)
            {
                var baseAvatarUrl = _configuration["CloudinarySettings:BaseAvatarUrl"];
                var avatarPath = $"{baseAvatarUrl}/avatar/default.jpg";
                user.Avatar = avatarPath;
            }
            user.PhoneNumber=model.PhoneNumber;
            if (model.PhoneNumber != null) user.PhoneNumberConfirmed = true;
            user.UserName = model.Username;
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
            if (model.Avatar != null)
            {
                var baseAvatarUrl = _fileService.UploadPictureAsync(model.Avatar);
                user.Avatar = baseAvatarUrl.Result;
                await _userManager.UpdateAsync(user);
            }
            return _mapper.Map<UserResponse>(user);
        }
        public async Task ImportEnrollmentsFromExcelAsync(ImportUserFileRequest file)
        {
            if (file == null || file.File.Length == 0)
                throw new AppException(ErrorCode.NoFile);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var emailsInFile = new HashSet<string>();

            using (var stream = new MemoryStream())
            {
                await file.File.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    // Collect all emails from Excel
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var email = worksheet.Cells[row, 3].Text.Trim();
                        if (!string.IsNullOrEmpty(email))
                            emailsInFile.Add(email);
                    }

                    // Load all existing users into dictionary
                    var existingUsers = _userManager.Users
                        .Where(u => emailsInFile.Contains(u.Email!) && u.Email != null)
                        .ToList()
                        .ToDictionary(u => u.Email!, u => u);

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var studentId = worksheet.Cells[row, 1].Text.Trim();
                        var fullName = worksheet.Cells[row, 2].Text.Trim();
                        var email = worksheet.Cells[row, 3].Text.Trim();
                        var termSeason = worksheet.Cells[row, 4].Text.Trim();
                        var termYearStr = worksheet.Cells[row, 5].Text.Trim();

                        //find and set termId 
                        var termID = await _termRepository.GetTermIdAsync(termSeason, int.Parse(termYearStr));
                        if (termID == Guid.Empty)
                        {
                            var starDate = await _termRepository.GetLatestTermEndDateAsync();
                            //create new term
                            var newTerm = new Term
                            {
                                Season = termSeason,
                                Year = int.Parse(termYearStr),
                                StartDate = starDate,
                                EndDate = starDate.AddMonths(4),
                                IsActive = true
                            };
                            termID = newTerm.Id;
                        }

                        if (string.IsNullOrEmpty(email)) continue;

                        int.TryParse(termYearStr, out int termYear);
                        if (existingUsers.TryGetValue(email, out var existingUser))
                        {
                            //User has an account but has never used it => delete account , create again
                            if (!existingUser.EmailConfirmed)
                            {
                                var deleteResult = await _userManager.DeleteAsync(existingUser);
                                if (!deleteResult.Succeeded)
                                {
                                    continue;
                                }
                                existingUsers.Remove(email); 
                            }
                            else // user has an account and used it => reset password and open an account
                            {
                                // Update existing confirmed user
                                existingUser.FullName = fullName;
                                existingUser.StudentId = studentId;
                                existingUser.TermId = termID;
                                existingUser.PastTerms = existingUser.PastTerms == null
                                    ? $"{termSeason} {termYear}"
                                    : $"{existingUser.PastTerms}, {termSeason} {termYear}";

                                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                                await _mailService.SendReactivationEmail(
                                    email, existingUser.UserName!, existingUser.FullName, resetToken);

                                continue;
                            }
                        }

                        // Add new user 
                        var newUser = new User
                        {
                            StudentId = studentId,
                            FullName = fullName,
                            Email = email,
                            UserName = email,
                            EmailConfirmed = false,
                            TermId = termID,
                            PastTerms = $"{termSeason} {termYear}",
                        };

                        var newPass = GenerateRandom.GenerateRandomNumber();
                        var createResult = await _userManager.CreateAsync(newUser, newPass);
                        if (createResult.Succeeded)
                        {
                            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(newUser);
                            await _mailService.SendWelcomeEmail(email, fullName, resetToken);
                        }
                        else
                        {
                        }
                    }
                }
            }
        }

        public async Task ConfirmEmailAndSetPasswordAsync(ActivateAccountRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.UserName!.Equals(user.Email))
            {
                throw new AppException(ErrorCode.InvalidEmail);
            }
            // if have username : change username
            if (request.Username != null)
            {
                user.UserName = request.Username;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    throw new AppException(ErrorCode.UsernameExists);
            }
            //reset password
            var rsPassword = await _userManager.ResetPasswordAsync(
                    user, request.TokenResetPassword, request.NewPassword);
            if (!rsPassword.Succeeded)
            {
                throw new AppException(ErrorCode.CannotResetPassword);
            }
        }

        public async Task ResetPassword(ResetPasswordRequest request)
        {
            //find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new AppException(ErrorCode.InvalidEmail);
            }
            //reset password
            var result = await _userManager.ResetPasswordAsync(
                    user, request.TokenResetPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new AppException(ErrorCode.CannotResetPassword);
            }
        }

        public Task SendMailResetPassword(string EmailOrUsername)
        {
            var user = _userManager.Users
                .FirstOrDefault(u => u.Email == EmailOrUsername || u.UserName == EmailOrUsername);
            if (user == null)
            {
                throw new AppException(ErrorCode.InvalidEmailOrUsername);
            }
            var resetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;
            return _mailService.SendResetPasswordEmail(user.Email!, resetToken);
        }
    }
}
