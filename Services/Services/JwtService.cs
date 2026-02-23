using Data.Interfaces;
using Data.Models;
using DTO.Responses;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Services
{
    public class JwtService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRefreshTokensRepository _refreshTokensRepository;

        private readonly IConfiguration _configuration;

        public JwtService(UserManager<User> userManager, IRefreshTokensRepository refreshTokensRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _refreshTokensRepository = refreshTokensRepository;
            _configuration = configuration;
        }

        public async Task<AuthTokensResponse> GenerateTokensAsync(User user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);

            var refreshTokenEntity = GenerateRefreshToken(user);

            var rawRefreshToken = refreshTokenEntity.Token;

            refreshTokenEntity.Token = ComputeSha256Hash(refreshTokenEntity.Token);

            await _refreshTokensRepository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthTokensResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
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
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshTokenString = Convert.ToBase64String(tokenBytes);

            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenString,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
        }

        public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken)
        {
            var hashedToken = ComputeSha256Hash(refreshToken);
            var tokenEntity = await _refreshTokensRepository.GetRefreshTokenAsync(hashedToken);

            if (tokenEntity is null || !tokenEntity.IsActive)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            if(tokenEntity.Expires < DateTime.UtcNow)
            {
                RevokeRefreshToken(tokenEntity);
                throw new SecurityTokenException("Refresh token has expired");
            }

            if(tokenEntity.Expires < DateTime.UtcNow.AddHours(12))
            {
                tokenEntity.Expires = DateTime.UtcNow.AddDays(7);
            }
            return tokenEntity;
        }

        public void RevokeRefreshToken(RefreshToken token)
        {
            token.Revoked = DateTime.UtcNow;
        }

        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
