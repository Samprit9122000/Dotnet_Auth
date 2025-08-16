using Dotnet_Auth.Models;
using Dotnet_Auth.ViewModels;

namespace Dotnet_Auth.Iservices
{
    public interface IUserService
    {
        Task<BaseResponse> Register(RegisterRequest request);
        Task<TokenModel> Login(LoginRequest request);
        Task<List<common_data>> GetAllDataAfterAuthorization();
        Task<TokenModel> TokenRefresh(string refresh_token, string user_email);
    }
}
