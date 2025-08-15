using Dotnet_Auth.Data;
using Dotnet_Auth.Iservices;
using Dotnet_Auth.Models;
using Dotnet_Auth.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;  // need System.IdentityModel.Tokens.Jwt


namespace Dotnet_Auth.Services
{
    public class UserService : IUserService
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public UserService(AppDbContext dbContext, IConfiguration configuration)
        {
            _context = dbContext;
            _configuration = configuration;
        }

        public async Task<BaseResponse> Register(RegisterRequest request)
        {
            var existing_user = _context.user_infos.AsNoTracking().FirstOrDefault(u => u.email == request.email);
            if (existing_user != null)
            {
                return (new BaseResponse
                {
                    success = false,
                    message = "User already exists"
                });
            }

            var user = new user_info
            {
                user_name = request.user_name,
                email = request.email,
                password = new PasswordHasher<user_info>().HashPassword(null, request.password) // In a real application, ensure to hash the password
            };

            await _context.user_infos.AddAsync(user);
            await _context.SaveChangesAsync();

            return new BaseResponse
            {
                success = true,
                message = "User registered successfully"
            };
        }


        public async Task<TokenModel> Login(LoginRequest request)
        {
            if (request.email == null || request.password == null)
                return (new TokenModel()
                {
                    meta_data = new BaseResponse
                    {
                        success = false,
                        message = "Email and password cannot be null"
                    },

                });


            // hash password comparision
            var check_password = new PasswordHasher<user_info>();
            var user = await _context.user_infos.AsNoTracking().FirstOrDefaultAsync(u => u.email == request.email);
            var verify_password = check_password.VerifyHashedPassword(null, user.password, request.password) == PasswordVerificationResult.Success;
            if (!verify_password)
            {
                return (new TokenModel()
                {
                    meta_data = new BaseResponse
                    {
                        success = false,
                        message = "User Not Found"
                    },

                });
            }

            return new TokenModel
            {
                meta_data = new BaseResponse
                {
                    success = true,
                    message = "Login successful"
                },
                token = GenerateToken(user) // Call the method to generate a token
            };

            // Generate a token (this is a placeholder, implement your token generation logic)


            //throw new NotImplementedException();
        }


        public async Task<List<common_data>> GetAllDataAfterAuthorization() 
        {

            var data = await _context.common_data
                .Include(d => d.user)
                .ToListAsync();
            return data;
        }


        #region Private method like generate token

        private string GenerateToken(user_info user)
        {
            var claims = new List<Claim>
            {
                new Claim("username", user.user_name),
                new Claim("useremail", user.email)

            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["token_creds:BaseKey"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token_descriptor = new JwtSecurityToken(
                issuer: _configuration["token_creds:Issuer"],
                audience: _configuration["token_creds:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token_descriptor); ;
        }

        #endregion

    }
}
