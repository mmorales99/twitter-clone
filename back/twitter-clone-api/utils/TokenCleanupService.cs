using Domain.Interfaces;

namespace twitter_clone_api.utils;

public class TokenCleanupService(IServiceProvider services) : BackgroundService
{
    private readonly IServiceProvider _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenRepository>();
            await repo.DeleteExpiredTokensAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}