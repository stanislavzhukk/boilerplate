using Data.Models;
using DTO.Requests;
using DTO.Responses;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthTokensResponse> LoginAsync(string email, string password);
        Task<AuthTokensResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<User> RegisterAsync(RegisterRequest request);
        Task<string?> RefreshAccessTokenAsync(string refreshToken);
    }
}
