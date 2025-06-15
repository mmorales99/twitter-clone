using Domain.DTO;

namespace application.cache.interfaces;

public interface IHashtagCache
{
    Task<List<HashtagDto>?> GetHashtagsAsync(int page, int pageSize);
    Task SetHashtagsAsync(int page, int pageSize, List<HashtagDto> hashtags, TimeSpan expiration);
    Task InvalidateAllAsync();
}