using Dotnet_Auth.Data;
using Dotnet_Auth.Iservices;
using Dotnet_Auth.Models;
using Dotnet_Auth.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;  // need System.IdentityModel.Tokens.Jwt


namespace Dotnet_Auth.Services
{
    public class UserService : IUserService
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(AppDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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
                token = GenerateToken(user), // Call the method to generate a token
                refresh_token = await GenerateAndSaveRefreshToken(GenerateToken(user), user),
                info = new RegisterRequest
                {
                    user_name = user.user_name,
                    email = user.email
                }
            };

            // Generate a token (this is a placeholder, implement your token generation logic)


            //throw new NotImplementedException();
        }




        /// <summary>
        /// Token refresh
        /// </summary>
        /// <returns></returns>
        public async Task<TokenModel> TokenRefresh( string refresh_token, string user_email)
        {
            var user_id = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var user = await _context.user_infos.AsNoTracking().FirstOrDefaultAsync(u => u.email == user_email);
            if(user == null)
            {
                return new TokenModel
                {
                    meta_data = new BaseResponse
                    {
                        success = false,
                        message = "No user found"
                    }
                };
            }
            if (user.refresh_token == null || user.refresh_token != refresh_token)
            {
                return new TokenModel
                {
                    meta_data = new BaseResponse
                    {
                        success = false,
                        message = "Invalid refresh token"
                    }
                };
            }

            // Generate a new token and refresh token
            var new_access_token = GenerateToken(user);
            var new_refresh_token = await GenerateAndSaveRefreshToken(new_access_token, user);

            // Update the user with the new refresh token
            user.refresh_token = new_refresh_token;
            _context.user_infos.Update(user);
            await _context.SaveChangesAsync();

            return new TokenModel
            {
                meta_data = new BaseResponse
                {
                    success = true,
                    message = "Token refreshed successfully"
                },
                token = new_access_token,
                refresh_token = new_refresh_token,
                info = new RegisterRequest{
                    user_name = user.user_name,
                    email = user.email
                }
            };
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
                new Claim("useremail", user.email),
                new Claim(ClaimTypes.Role,user.role ?? ""),
                new Claim("RefreshToken",user.refresh_token ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString())

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


        private async Task<string> GenerateAndSaveRefreshToken(string access_token, user_info user)
        {
            var random_bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random_bytes);
            var refresh_token = Convert.ToBase64String(random_bytes);

            user.refresh_token = refresh_token;
            _context.user_infos.Update(user);
            await _context.SaveChangesAsync();

            return refresh_token;
        }
        #endregion

    }
}
