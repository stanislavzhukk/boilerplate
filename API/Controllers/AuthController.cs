using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using DTO.Requests;
using Services.Services;
using Microsoft.AspNetCore.Identity.Data;

namespace API.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTO.Requests.RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new { message = "Registration successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return StatusCode(500, new { message = $"An error occurred during registration.", details = $"{ex.Message}" });
            }
        }

        //public async Task<Result<T>> Login([FromBody] ...)
        // For simplicity, we return IActionResult here, but in a real application, consider using a consistent response wrapper
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTO.Requests.LoginRequest request)
        {
            try
            {
                var tokens = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", request.Email);
                return Unauthorized(new { message = $"Invalid email or password.", details = $"{ex.Message}" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var accessToken = await _authService.RefreshAccessTokenAsync(request.RefreshToken);
                if (accessToken == null)
                {
                    return Unauthorized();
                }
                return Ok(new { AccessToken = accessToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed for refresh token: {RefreshToken}", request.RefreshToken);
                return Unauthorized(new { message = ex.Message });

            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                await _authService.LogoutAsync(request.RefreshToken);
                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for refresh token: {RefreshToken}", request.RefreshToken);
                return StatusCode(500, new { message = "An error occurred during logout.", details = $"{ex.Message}" });
            }
        }
    }
}
