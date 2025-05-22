using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using taskflow_api.Entity;
using taskflow_api.Enums;
using taskflow_api.Exceptions;
using taskflow_api.Helpers;
using taskflow_api.Model.Request;

namespace taskflow_api.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, 
            IConfiguration configuration, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _env = env;
        }

        public async Task<string> Login(LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                throw new AppException(ErrorCode.InvalidEmail);
            }
            if (!user.IsActive)
            {
                throw new AppException(ErrorCode.AccountBanned);
            }
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                throw new AppException(ErrorCode.InvalidPassword);
            }
            var authClaims = new List<Claim>
            {
                new Claim("ID", user.Id.ToString()),
                new Claim("Email", user.Email !),
                new Claim("Role", user.Role.ToString()),
                new Claim("Fullname", user.FullName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var authenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(authenKey), SecurityAlgorithms.HmacSha256)
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
    }
}
