using Application.DTO.Responses;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<RefreshToken> GenerateRefreshTokenAsync(User user);
    }
}
