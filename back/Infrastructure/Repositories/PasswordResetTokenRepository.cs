using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

[Register(ServiceLifetime.Singleton)]
public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IMongoCollection<PasswordResetToken> _tokens;

    public PasswordResetTokenRepository(IMongoClient client)
    {
        var db = client.GetDatabase("twitter_clone");
        _tokens = db.GetCollection<PasswordResetToken>("password_reset_tokens");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Índice para expiración automática (TTL de 1 día tras expiresAt)
        var indexKeys = Builders<PasswordResetToken>.IndexKeys.Ascending(t => t.ExpiresAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
        _tokens.Indexes.CreateOne(new CreateIndexModel<PasswordResetToken>(indexKeys, indexOptions));
    }

    public async Task SaveAsync(PasswordResetToken token)
    {
        await _tokens.InsertOneAsync(token);
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _tokens.Find(t => t.Token == token).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        await _tokens.ReplaceOneAsync(t => t.Token == token.Token, token);
    }

    public async Task DeleteExpiredTokensAsync()
    {
        await _tokens.DeleteManyAsync(t => t.ExpiresAt < DateTime.UtcNow);
    }
}
