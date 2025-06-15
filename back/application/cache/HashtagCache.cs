using application.cache.interfaces;
using AutoRegister;
using Domain.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace application.cache;

[Register(ServiceLifetime.Singleton)]
public class HashtagCache(IMemoryCache cache) : IHashtagCache
{
    private readonly IMemoryCache _cache = cache;
    private const string CacheKeyPrefix = "hashtags_page_";

    public Task<List<HashtagDto>?> GetHashtagsAsync(int page, int pageSize)
    {
        var key = $"{CacheKeyPrefix}{page}_{pageSize}";
        _cache.TryGetValue(key, out List<HashtagDto>? result);
        return Task.FromResult(result);
    }

    public Task SetHashtagsAsync(int page, int pageSize, List<HashtagDto> hashtags, TimeSpan expiration)
    {
        var key = $"{CacheKeyPrefix}{page}_{pageSize}";
        _cache.Set(key, hashtags, expiration);
        return Task.CompletedTask;
    }

    public Task InvalidateAllAsync()
    {
        // Si usas MemoryCache, deberías llevar un registro de las claves para invalidarlas.
        // Para simplificar, puedes reiniciar el cache o implementar una lógica de invalidación más avanzada.
        return Task.CompletedTask;
    }
}
