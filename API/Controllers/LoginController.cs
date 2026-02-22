using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginService _loginService;
        public LoginController(ILogger<LoginController> logger, ILoginService loginService)
        {
            _logger = logger;
            _loginService = loginService;
        }

        // Consider using a DTO for the login request instead of raw parameters
        //public async Task<IActionResult> Login([FromBody] string email, [FromBody]string password)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var token = await _loginService.LoginAsync(email, password);

            if (!string.IsNullOrEmpty(token))
            {
                return Ok(new { Token = token });
            }
            return Unauthorized("Invalid email or password");
        }
    }
}
