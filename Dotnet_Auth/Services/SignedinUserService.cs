using Dotnet_Auth.Iservices;
using System.Security.Claims;

namespace Dotnet_Auth.Services
{
    public class SignedinUserService : ISignedinUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Implement interface properties
        public string username { get; set; }
        public string email { get; set; }

        public SignedinUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            username = _httpContextAccessor.HttpContext?.User.FindFirst("username")?.Value ?? string.Empty;
            email = _httpContextAccessor.HttpContext?.User.FindFirst("useremail")?.Value ?? string.Empty;
        }

    }
}
