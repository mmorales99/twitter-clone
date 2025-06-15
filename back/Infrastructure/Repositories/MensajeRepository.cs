using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

[Register(ServiceLifetime.Scoped)]
public class MensajeRepository : IMensajeRepository
{
    private readonly IMongoCollection<Mensaje> _mensajes;

    public MensajeRepository(IMongoClient client)
    {
        var db = client.GetDatabase("twitter_clone");
        _mensajes = db.GetCollection<Mensaje>("mensajes");
    }

    public async Task<Mensaje?> GetByIdAsync(string id)
    {
        return await _mensajes.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(Mensaje mensaje)
    {
        var result = await _mensajes.ReplaceOneAsync(m => m.Id == mensaje.Id, mensaje);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _mensajes.DeleteOneAsync(m => m.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<List<Mensaje>> GetRespuestasAsync(string mensajeId, int page, int pageSize)
    {
        return await _mensajes
            .Find(m => m.ReplyTo == mensajeId)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Mensaje> PublicarAsync(Mensaje mensaje)
    {
        await _mensajes.InsertOneAsync(mensaje);
        return mensaje;
    }

    public async Task<List<Mensaje>> ObtenerPorUsuarioIdsAsync(IEnumerable<string> userIds, int page, int pageSize)
    {
        var filter = Builders<Mensaje>.Filter.In(m => m.UserId, userIds);
        return await _mensajes.Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<Mensaje>> ObtenerPorHashtagAsync(string hashtag, int page, int pageSize)
    {
        var filter = Builders<Mensaje>.Filter.AnyEq(m => m.Hashtags, hashtag.ToLower());
        return await _mensajes.Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<Mensaje>> GetMensajesPorHashtagAsync(string hashtag, int page, int pageSize)
    {
        // Suponiendo que cada mensaje tiene una lista de hashtags en un campo Hashtags
        var filter = Builders<Mensaje>.Filter.AnyEq(m => m.Hashtags, hashtag);
        return await _mensajes
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }
}
