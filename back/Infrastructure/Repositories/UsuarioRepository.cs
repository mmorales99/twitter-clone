using AutoRegister;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

[Register(ServiceLifetime.Scoped)]
public class UsuarioRepository : IUsuarioRepository
{
    private readonly IMongoCollection<Usuario> _usuarios;

    public UsuarioRepository(IMongoClient client)
    {
        var db = client.GetDatabase("twitter_clone");
        _usuarios = db.GetCollection<Usuario>("usuarios");
    }

    public async Task<Usuario?> GetByIdAsync(string id) =>
        await _usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<Usuario?> GetByUsernameAsync(string username) =>
        await _usuarios.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<Usuario?> GetByEmailAsync(string mail) =>
        await _usuarios.Find(u => u.Email == mail).FirstOrDefaultAsync();

    public async Task AddAsync(Usuario usuario) =>
        await _usuarios.InsertOneAsync(usuario);

    public async Task<IEnumerable<Usuario>> GetAllAsync() =>
        await _usuarios.Find(_ => true).ToListAsync();

    public async Task FollowUser(string followerId, string userToFollowId)
    {
        var filterFollower = Builders<Usuario>.Filter.Eq(u => u.Id, followerId);
        var updateFollower = Builders<Usuario>.Update.AddToSet(u => u.FollowingIds, userToFollowId);

        var filterFollowed = Builders<Usuario>.Filter.Eq(u => u.Id, userToFollowId);
        var updateFollowed = Builders<Usuario>.Update.AddToSet(u => u.FollowerIds, followerId);

        using var session = await _usuarios.Database.Client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await _usuarios.UpdateOneAsync(session, filterFollower, updateFollower);
            await _usuarios.UpdateOneAsync(session, filterFollowed, updateFollowed);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task UnfollowUser(string followerId, string userToUnfollowId)
    {
        var filterFollower = Builders<Usuario>.Filter.Eq(u => u.Id, followerId);
        var updateFollower = Builders<Usuario>.Update.Pull(u => u.FollowingIds, userToUnfollowId);

        var filterUnfollowed = Builders<Usuario>.Filter.Eq(u => u.Id, userToUnfollowId);
        var updateUnfollowed = Builders<Usuario>.Update.Pull(u => u.FollowerIds, followerId);

        using var session = await _usuarios.Database.Client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await _usuarios.UpdateOneAsync(session, filterFollower, updateFollower);
            await _usuarios.UpdateOneAsync(session, filterUnfollowed, updateUnfollowed);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<List<Usuario>> GetFollowers(string userId)
    {
        // Buscar usuarios que tengan userId en su lista de FollowingIds
        var filter = Builders<Usuario>.Filter.AnyEq(u => u.FollowingIds, userId);
        return await _usuarios.Find(filter).ToListAsync();
    }

    public async Task<List<Usuario>> GetFollowing(string userId)
    {
        // Buscar el usuario y devolver los usuarios que sigue
        var usuario = await _usuarios.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (usuario == null || usuario.FollowingIds == null || usuario.FollowingIds.Count == 0)
            return new List<Usuario>();

        var filter = Builders<Usuario>.Filter.In(u => u.Id, usuario.FollowingIds);
        return await _usuarios.Find(filter).ToListAsync();
    }

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        var filter = Builders<Usuario>.Filter.Eq(u => u.Id, usuario.Id);
        var result = await _usuarios.ReplaceOneAsync(filter, usuario);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<Usuario>.Filter.Eq(u => u.Id, id);
        var result = await _usuarios.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}