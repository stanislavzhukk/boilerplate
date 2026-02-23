using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IRefreshTokensRepository
    {
        Task AddRefreshTokenAsync(RefreshToken token);
        Task DeleteExpiredAndRevokedAsync();
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
        Task UpdateAsync(RefreshToken tokenEntity);
    }
}
