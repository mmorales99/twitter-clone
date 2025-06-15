using Domain.Entities;

namespace Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task SaveAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task UpdateAsync(PasswordResetToken token);
    Task DeleteExpiredTokensAsync();
}
