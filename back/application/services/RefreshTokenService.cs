using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace application.services;

[Register(ServiceLifetime.Scoped)]
public class RefreshTokenService(IRefreshTokenRepository repo)
{
    public async Task<RefreshToken?> ValidateAndGetRefreshTokenAsync(string token)
    {
        var storedToken = await repo.GetByTokenAsync(token);
        if (storedToken == null ||
            storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
            return null;

        return storedToken;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await repo.SaveAsync(refreshToken);
        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        await repo.RevokeAsync(token);
    }
}
