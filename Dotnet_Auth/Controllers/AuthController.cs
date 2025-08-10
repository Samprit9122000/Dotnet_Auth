using Dotnet_Auth.Iservices;
using Dotnet_Auth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Here you would typically save the user to a database
            // For demonstration, we will just return a success message
            var response = await _userService.Register(request);
            if((bool)response.success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Here you would typically validate the user credentials
            // For demonstration, we will return a dummy token
            var response = await _userService.Login(request);
            if((bool)response.meta_data.success)
            {
                return Ok(response);
            }
            return Unauthorized(response);
        }

        [Authorize]
        [HttpGet("FetchAllData/AfterAuthorization")]
        public async Task<IActionResult> FetchAllDataAfterAuthorization()
        {
            // This endpoint is protected and requires authorization
            var data = await _userService.GetAllDataAfterAuthorization();
            
            return Ok(data);
            
        }


    }
}
