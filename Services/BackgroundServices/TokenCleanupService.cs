
using Microsoft.Extensions.Hosting;
using Data.Interfaces;

namespace Services.BackgroundServices
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IRefreshTokensRepository _repository;

        public TokenCleanupService(IRefreshTokensRepository repository)
        {
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Удаляем expired и revoked токены
                await _repository.DeleteExpiredAndRevokedAsync();

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
