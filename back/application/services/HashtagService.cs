using application.cache.interfaces;
using AutoRegister;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace application.services;

[Register(ServiceLifetime.Scoped)]
public class HashtagService(
    IHashtagRepository hashtagRepo, 
    IMensajeRepository mensajeRepo, 
    IHashtagCache hashtagCache
)
{
    public async Task<List<Mensaje>> GetMensajesPorHashtagAsync(string nombre, int page, int pageSize)
    {
        return await mensajeRepo.GetMensajesPorHashtagAsync(nombre, page, pageSize);
    }

    public async Task<List<HashtagDto>> GetHashtagsAsync(int page, int pageSize)
    {
        // 1. Intenta obtener de caché
        var cached = await hashtagCache.GetHashtagsAsync(page, pageSize);
        if (cached is not null)
            return cached;

        // 2. Si no está en caché, consulta la base de datos
        var hashtags = await hashtagRepo.GetAllAsync(page, pageSize);
        var dtos = hashtags.Select(h => new HashtagDto(h.Tag, h.Contador)).ToList();

        // 3. Guarda en caché
        await hashtagCache.SetHashtagsAsync(page, pageSize, dtos, TimeSpan.FromMinutes(5));
        return dtos;
    }
}