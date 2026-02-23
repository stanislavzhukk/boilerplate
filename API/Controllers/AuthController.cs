using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Services;

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

        //public async Task<Result<T>> Login([FromBody] ...)
        // For simplicity, we return IActionResult here, but in a real application, consider using a consistent response wrapper
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            try
            {
                var tokens = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password." });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var tokens = await _authService.RefreshAsync(request.RefreshToken);
                if (tokens == null)
                    return Unauthorized(new { message = "Invalid or expired refresh token." });

                return Ok(new { tokens.AccessToken, tokens.RefreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed for refresh token: {RefreshToken}", request.RefreshToken);
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }
        }
    }
}
