using Domain.Entities;

namespace Domain.Interfaces;

public interface IHashtagRepository
{
    Task CrearOSumarAsync(string tag);
    Task<List<Hashtag>> GetAllAsync();
    Task<Hashtag?> GetByTagAsync(string tag);
    Task<List<Hashtag>> GetAllAsync(int page, int pageSize);
    Task<Hashtag?> GetByNombreAsync(string nombre);
    Task IncrementarUsoAsync(string nombre);
}