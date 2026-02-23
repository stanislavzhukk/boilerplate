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

namespace Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration,
            IRefreshTokensRepository refreshTokens)
        {
            _userManager = userManager;
            _configuration = configuration;
            _refreshTokensRepository = refreshTokens;
        }

        public async Task<AuthTokensResponse> LoginAsync(string email, string password)
        {
            var user = await ValidateUserAsync(email, password)
                ?? throw new SecurityTokenException("Invalid credentials");

            return await GenerateTokensAsync(user);
        }

        public async Task<AuthTokensResponse> RefreshAsync(string refreshToken)
        {
            var tokenEntity = await ValidateRefreshTokenAsync(refreshToken);

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            RevokeRefreshToken(tokenEntity);
            await _refreshTokensRepository.UpdateAsync(tokenEntity);

            return await GenerateTokensAsync(user);
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

        private async Task<AuthTokensResponse> GenerateTokensAsync(User user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);

            var refreshTokenEntity = GenerateRefreshToken(user);
            await _refreshTokensRepository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthTokensResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenEntity.Token
            };
        }

        private async Task<string> GenerateAccessTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var secret = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey not configured");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationInMinutes");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }

        private RefreshToken GenerateRefreshToken(User user)
        {
            return new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = user.Id
            };
        }

        private async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _refreshTokensRepository.GetRefreshTokenAsync(refreshToken);

            if (tokenEntity is null || !tokenEntity.IsActive)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            return tokenEntity;
        }

        private void RevokeRefreshToken(RefreshToken token)
        {
            token.Revoked = DateTime.UtcNow;
        }

        public Task LogoutAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}