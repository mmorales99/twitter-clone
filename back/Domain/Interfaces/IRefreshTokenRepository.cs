using Domain.Entities;

namespace Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(string token);
}
