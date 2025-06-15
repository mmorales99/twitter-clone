using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

[Register(ServiceLifetime.Scoped)]
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _refreshTokens;

    public RefreshTokenRepository(IMongoClient client)
    {
        var db = client.GetDatabase("twitter_clone");
        _refreshTokens = db.GetCollection<RefreshToken>("refreshTokens");
    }

    public async Task SaveAsync(RefreshToken token)
    {
        await _refreshTokens.InsertOneAsync(token);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _refreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();
    }

    public async Task RevokeAsync(string token)
    {
        var update = Builders<RefreshToken>.Update
            .Set(rt => rt.IsRevoked, true)
            .Set(rt => rt.RevokedAt, DateTime.UtcNow);

        await _refreshTokens.UpdateOneAsync(rt => rt.Token == token, update);
    }
}