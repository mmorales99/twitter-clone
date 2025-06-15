using Domain.Entities;

namespace Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(string id);
    Task<Usuario?> GetByUsernameAsync(string username);
    Task AddAsync(Usuario usuario);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task FollowUser(string followerId, string userToFollowId);
    Task UnfollowUser(string followerId, string userToUnfollowId);
    Task<List<Usuario>> GetFollowers(string userId);
    Task<List<Usuario>> GetFollowing(string userId);
    Task<bool> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(string id);
    Task<Usuario?> GetByEmailAsync(string email);
}