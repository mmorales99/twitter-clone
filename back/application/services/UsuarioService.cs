using AutoRegister;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace application.services;

[Register(ServiceLifetime.Scoped)]
public class UsuarioService(
    IUsuarioRepository usuarioRepo,
    PasswordHasher<Usuario> passwordHasher
)
{
    public async Task<Usuario?> GetUsuarioPorUsername(string username) 
        => await usuarioRepo.GetByUsernameAsync(username);
    public async Task<Usuario?> GetPerfilAsync(string userId) 
        => await usuarioRepo.GetByIdAsync(userId);

    // Crear un nuevo usuario
    public async Task<Usuario> CrearUsuarioAsync(RegisterDto dto)
    {
        // Comprueba si ya existe usuario o email
        if (await usuarioRepo.GetByUsernameAsync(dto.Username) != null || await usuarioRepo.GetByEmailAsync(dto.Email) != null)
            return null;

        var usuario = new Usuario
        {
            Username = dto.Username,
            Email = dto.Email,
            Role = "Usuario", // Por defecto
            CreatedAt = DateTime.UtcNow
        };

        usuario.PasswordHash = passwordHasher.HashPassword(usuario, dto.Password);

        await usuarioRepo.AddAsync(usuario);
        return usuario;
    }

    // Obtener usuario por Id
    public async Task<Usuario?> GetByIdAsync(string id)
    {
        return await usuarioRepo.GetByIdAsync(id);
    }

    // Obtener usuario por username
    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await usuarioRepo.GetByUsernameAsync(username);
    }

    // Obtener todos los usuarios (paginado)
    public async Task<List<Usuario>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        // Si tu repositorio soporta paginación, implementa aquí
        var all = await usuarioRepo.GetAllAsync();
        return all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    // Seguir a otro usuario
    public async Task FollowUserAsync(string followerId, string userToFollowId)
    {
        await usuarioRepo.FollowUser(followerId, userToFollowId);
    }

    // Dejar de seguir a otro usuario
    public async Task UnfollowUserAsync(string followerId, string userToUnfollowId)
    {
        await usuarioRepo.UnfollowUser(followerId, userToUnfollowId);
    }

    // Obtener seguidores de un usuario
    public async Task<List<Usuario>> GetFollowersAsync(string userId)
    {
        return await usuarioRepo.GetFollowers(userId);
    }

    // Obtener seguidos de un usuario
    public async Task<List<Usuario>> GetFollowingAsync(string userId)
    {
        return await usuarioRepo.GetFollowing(userId);
    }

    // Actualizar datos de un usuario
    public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
    {
        // Implementa el método UpdateAsync en el repositorio si no existe
        return await usuarioRepo.UpdateAsync(usuario);
    }

    // Eliminar usuario
    public async Task<bool> DeleteUsuarioAsync(string id)
    {
        // Implementa el método DeleteAsync en el repositorio si no existe
        return await usuarioRepo.DeleteAsync(id);
    }

    public string HashPassword(string password, Usuario usuario)
    {
        return passwordHasher.HashPassword(usuario, password);
    }

    public async Task<Usuario?> AuthenticateAsync(string username, string password)
    {
        var usuario = await usuarioRepo.GetByUsernameAsync(username);
        if (usuario == null)
            return null;

        var result = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? usuario : null;
    }

    public async Task<bool> CambiarPasswordAsync(string userId, string passwordActual, string passwordNueva)
    {
        var usuario = await usuarioRepo.GetByIdAsync(userId);
        if (usuario == null) return false;

        var result = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, passwordActual);
        if (result != PasswordVerificationResult.Success) return false;

        usuario.PasswordHash = passwordHasher.HashPassword(usuario, passwordNueva);
        await usuarioRepo.UpdateAsync(usuario);
        return true;
    }

    public async Task<bool> ActualizarPerfilAsync(string userId, ActualizarPerfilDto dto)
    {
        var usuario = await usuarioRepo.GetByIdAsync(userId);
        if (usuario == null) return false;

        if (!string.IsNullOrEmpty(dto.Email)) usuario.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.Biografia)) usuario.Biografia = dto.Biografia;
        if (!string.IsNullOrEmpty(dto.Nombre)) usuario.Nombre = dto.Nombre;

        await usuarioRepo.UpdateAsync(usuario);
        return true;
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await usuarioRepo.GetByEmailAsync(email);
    }
}