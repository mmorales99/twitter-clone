using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace application.services;

[Register(ServiceLifetime.Scoped)]
public class MensajeService(
    IMensajeRepository mensajeRepo,
    IUsuarioRepository usuarioRepo,
    IHashtagRepository hashtagRepo)
{
    public async Task<List<Mensaje>> GetMensajesPorUsuarioAsync(string userId, int page = 1, int pageSize = 20)
        => await mensajeRepo.ObtenerPorUsuarioIdsAsync([userId], page, pageSize);

    public async Task<Mensaje> PublicarRespuestaAsync(string userId, string contenido, string mensajeOriginalId)
    {
        var mensaje = new Mensaje
        {
            UserId = userId,
            Content = contenido,
            CreatedAt = DateTime.UtcNow,
            ReplyTo = mensajeOriginalId,
            Hashtags = ExtraerHashtags(contenido)
        };
        await mensajeRepo.PublicarAsync(mensaje);
        // Actualizar hashtags, etc.
        return mensaje;
    }

    private static List<string> ExtraerHashtags(string contenido)
    {
        // Esta expresión regular captura hashtags formados por letras, números y guiones bajos
        var matches = Regex.Matches(contenido, @"#\w+");
        // Devuelve una lista de hashtags en minúsculas y sin duplicados
        return [.. matches
            .Select(m => m.Value.ToLower())
            .Distinct()];
    }

    public async Task<Mensaje> PublicarMensajeAsync(string userId, string contenido, string? replyTo)
    {
        // Extraer hashtags
        var hashtags = Regex.Matches(contenido, @"#\w+")
                            .Select(m => m.Value.ToLower())
                            .Distinct()
                            .ToList();

        // Crear mensaje
        var mensaje = new Mensaje
        {
            UserId = userId,
            Content = contenido,
            CreatedAt = DateTime.UtcNow,
            Hashtags = hashtags,
            ReplyTo = replyTo
        };

        // Guardar mensaje
        await mensajeRepo.PublicarAsync(mensaje);

        // Actualizar hashtags
        foreach (var tag in hashtags)
        {
            await hashtagRepo.CrearOSumarAsync(tag);
        }

        return mensaje;
    }

    public async Task<List<Mensaje>> ObtenerTimelineAsync(string userId, int page, int pageSize)
    {
        var usuario = await usuarioRepo.GetByIdAsync(userId);
        if (usuario == null) return new List<Mensaje>();

        return await mensajeRepo.ObtenerPorUsuarioIdsAsync(usuario.FollowingIds, page, pageSize);
    }

    public async Task<List<Mensaje>> ObtenerPorHashtagAsync(string hashtag, int page, int pageSize)
    {
        return await mensajeRepo.ObtenerPorHashtagAsync(hashtag, page, pageSize);
    }

    public async Task<List<Mensaje>> GetTimelineAsync(string userId, int page = 1, int pageSize = 20)
    {
        var usuario = await usuarioRepo.GetByIdAsync(userId);
        if (usuario == null || usuario.FollowingIds == null || usuario.FollowingIds.Count == 0)
            return new List<Mensaje>();

        // Consulta a través del repositorio
        return await mensajeRepo.ObtenerPorUsuarioIdsAsync(usuario.FollowingIds, page, pageSize);
    }

    // Obtener mensajes por hashtag
    public async Task<List<Mensaje>> GetMensajesPorHashtagAsync(string hashtag, int page = 1, int pageSize = 20)
    {
        var tagFormateado = hashtag.StartsWith("#") ? hashtag.ToLower() : $"#{hashtag.ToLower()}";
        return await mensajeRepo.ObtenerPorHashtagAsync(tagFormateado, page, pageSize);
    }

    public async Task<Mensaje?> GetMensajePorIdAsync(string id)
    {
        return await mensajeRepo.GetByIdAsync(id);
    }

    public async Task<bool> ActualizarMensajeAsync(string mensajeId, string userId, string nuevoContenido)
    {
        var mensaje = await mensajeRepo.GetByIdAsync(mensajeId);
        if (mensaje == null || mensaje.UserId != userId)
            return false;

        mensaje.Content = nuevoContenido;
        return await mensajeRepo.UpdateAsync(mensaje);
    }

    public async Task<bool> EliminarMensajeAsync(string mensajeId, string userId, bool esAdmin)
    {
        var mensaje = await mensajeRepo.GetByIdAsync(mensajeId);
        if (mensaje == null)
            return false;
        if (mensaje.UserId != userId && !esAdmin)
            return false;

        return await mensajeRepo.DeleteAsync(mensajeId);
    }

    public async Task<List<Mensaje>> GetRespuestasAsync(string mensajeId, int page = 1, int pageSize = 20)
    {
        return await mensajeRepo.GetRespuestasAsync(mensajeId, page, pageSize);
    }
}