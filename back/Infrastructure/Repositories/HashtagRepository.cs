using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

[Register(ServiceLifetime.Scoped)]
public class HashtagRepository : IHashtagRepository
{
    private readonly IMongoCollection<Hashtag> _hashtags;

    public HashtagRepository(IMongoClient client)
    {
        var db = client.GetDatabase("twitter_clone");
        _hashtags = db.GetCollection<Hashtag>("hashtags");
    }

    public async Task CrearOSumarAsync(string tag)
    {
        var normalizedTag = tag.StartsWith("#") ? tag.ToLower() : $"#{tag.ToLower()}";
        var filter = Builders<Hashtag>.Filter.Eq(h => h.Tag, normalizedTag);
        var update = Builders<Hashtag>.Update
            .SetOnInsert(h => h.CreatedAt, DateTime.UtcNow)
            .Inc("Contador", 1);

        await _hashtags.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task<List<Hashtag>> GetAllAsync(int page, int pageSize)
    {
        return await _hashtags
            .Find(_ => true)
            .SortByDescending(h => h.Contador)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Hashtag?> GetByNombreAsync(string nombre)
    {
        return await _hashtags.Find(h => h.Tag == nombre).FirstOrDefaultAsync();
    }

    public async Task IncrementarUsoAsync(string nombre)
    {
        var update = Builders<Hashtag>.Update.Inc(h => h.Contador, 1);
        await _hashtags.UpdateOneAsync(
            h => h.Tag == nombre,
            update,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task<List<Hashtag>> GetAllAsync()
    {
        return await _hashtags.Find(_ => true).SortByDescending(h => h.CreatedAt).ToListAsync();
    }

    public async Task<Hashtag?> GetByTagAsync(string tag)
    {
        var normalizedTag = tag.StartsWith("#") ? tag.ToLower() : $"#{tag.ToLower()}";
        return await _hashtags.Find(h => h.Tag == normalizedTag).FirstOrDefaultAsync();
    }
}