namespace Dotnet_Auth.ViewModels
{
    public class TokenModel
    {
        public BaseResponse meta_data { get; set; }
        public string? token { get; set; }
        public string? refresh_token { get; set; }
        public RegisterRequest? info { get; set; }
    }
}
