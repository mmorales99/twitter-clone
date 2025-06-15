using Domain.Entities;

namespace Domain.Interfaces;

public interface IMensajeRepository
{
    Task<Mensaje> PublicarAsync(Mensaje mensaje);
    Task<List<Mensaje>> ObtenerPorUsuarioIdsAsync(IEnumerable<string> userIds, int page, int pageSize);
    Task<List<Mensaje>> ObtenerPorHashtagAsync(string hashtag, int page, int pageSize);
    Task<Mensaje?> GetByIdAsync(string id);
    Task<bool> UpdateAsync(Mensaje mensaje);
    Task<bool> DeleteAsync(string id);
    Task<List<Mensaje>> GetRespuestasAsync(string mensajeId, int page, int pageSize);
    Task<List<Mensaje>> GetMensajesPorHashtagAsync(string hashtag, int page, int pageSize);
}