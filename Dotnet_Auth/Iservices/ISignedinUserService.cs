namespace Dotnet_Auth.Iservices
{
    public interface ISignedinUserService
    {
        string email { get; set; }
        string username { get; set; }
    }
}
