using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DTO.Responses;
using Data.Interfaces;
using DTO.Requests;

namespace Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly JwtService _jwtService;

        public AuthService(
            UserManager<User> userManager,
            IRefreshTokensRepository refreshTokens,
            JwtService jwtService)
        {
            _userManager = userManager;
            _refreshTokensRepository = refreshTokens;
            _jwtService = jwtService;
        }

        public async Task<AuthTokensResponse> LoginAsync(string email, string password)
        {
            var user = await ValidateUserAsync(email, password)
                ?? throw new SecurityTokenException("Invalid credentials");

            return await _jwtService.GenerateTokensAsync(user);
        }

        public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
        {
            var tokenEntity = await _jwtService.ValidateRefreshTokenAsync(refreshToken);
            if (tokenEntity == null)
                return null;

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
            if (user == null)
                return null;

            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user);

            return newAccessToken;
        }

        public Task LogoutAsync(string refreshToken)
        {
            var tokenEntity = _refreshTokensRepository.GetRefreshTokenAsync(refreshToken).Result;  
            if (tokenEntity == null)
            {
                return Task.CompletedTask;
            }
            _jwtService.RevokeRefreshToken(tokenEntity);
            return Task.CompletedTask;
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            var user = _userManager.FindByEmailAsync(request.Email).Result;
            if(user != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            var userRecord = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                UserName = request.Name,
                NormalizedUserName = request.Name.ToUpper(),
                EmailConfirmed = false,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(userRecord, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }
            await _userManager.AddToRoleAsync(userRecord, "User");
            //verify email logic can be added here
            return userRecord;
        }


        private async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return null;
            }

            return user;
        }
    }
}