using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Domain.Entities;
using Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using application.services;
using System.Security.Claims;
using Domain.Interfaces;

namespace twitter_clone_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(
    UsuarioService usuarioService,
    EmailService emailService,
    IPasswordResetTokenRepository resetTokenRepo
) : ControllerBase
{

    // Obtener perfil de usuario
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPerfil(string id)
    {
        var usuario = await usuarioService.GetByIdAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    [Authorize]
    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ok = await usuarioService.ActualizarPerfilAsync(userId, dto);
        if (!ok) return BadRequest("Usuario no encontrado.");
        return NoContent();
    }

    [HttpPost("recuperar-password")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
    {
        var usuario = await usuarioService.GetByEmailAsync(dto.Email);
        if (usuario == null) return NoContent(); // No revelar si existe

        var token = Guid.NewGuid().ToString("N");
        var resetToken = new PasswordResetToken
        {
            Token = token,
            UserId = usuario.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        await resetTokenRepo.SaveAsync(resetToken);

        await emailService.SendPasswordResetEmail(usuario.Email, token);

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ConfirmarRecuperacion([FromBody] ConfirmarRecuperacionDto dto)
    {
        var resetToken = await resetTokenRepo.GetByTokenAsync(dto.Token);
        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
            return BadRequest("Token inválido o expirado.");

        var usuario = await usuarioService.GetByIdAsync(resetToken.UserId);
        if (usuario == null) return BadRequest("Usuario no encontrado.");

        // Valida la nueva contraseña con FluentValidation antes de continuar

        usuario.PasswordHash = usuarioService.HashPassword(dto.NuevaPassword, usuario);
        await usuarioService.UpdateUsuarioAsync(usuario);

        resetToken.IsUsed = true;
        await resetTokenRepo.UpdateAsync(resetToken);

        return NoContent();
    }

    [Authorize]
    [HttpPost("cambiar-password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ok = await usuarioService.CambiarPasswordAsync(userId, dto.PasswordActual, dto.PasswordNueva);
        if (!ok) return BadRequest("Contraseña actual incorrecta o usuario no encontrado.");
        return NoContent();
    }

    // Seguir a otro usuario
    [HttpPost("{id}/follow")]
    public async Task<IActionResult> FollowUser(string id, [FromBody] string userToFollowId)
    {
        await usuarioService.FollowUserAsync(id, userToFollowId);
        return NoContent();
    }

    // Dejar de seguir a otro usuario
    [HttpPost("{id}/unfollow")]
    public async Task<IActionResult> UnfollowUser(string id, [FromBody] string userToUnfollowId)
    {
        await usuarioService.UnfollowUserAsync(id, userToUnfollowId);
        return NoContent();
    }

    // Obtener seguidores
    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetFollowers(string id)
    {
        var seguidores = await usuarioService.GetFollowersAsync(id);
        return Ok(seguidores);
    }

    // Obtener seguidos
    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetFollowing(string id)
    {
        var seguidos = await usuarioService.GetFollowingAsync(id);
        return Ok(seguidos);
    }

    // Actualizar usuario
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUsuario(string id, [FromBody] ActualizarUsuarioDto dto, [FromServices] IValidator<ActualizarUsuarioDto> validator)
    {
        // Validación explícita
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        // Buscar usuario existente
        var usuarioExistente = await usuarioService.GetByIdAsync(id);
        if (usuarioExistente == null)
            return NotFound();

        // Actualizar solo los campos permitidos
        if (dto.Bio is not null)
            usuarioExistente.Bio = dto.Bio;
        if (dto.AvatarUrl is not null)
            usuarioExistente.AvatarUrl = dto.AvatarUrl;
        if (dto.Email is not null)
            usuarioExistente.Email = dto.Email;
        // Agrega aquí más campos según tu modelo

        var actualizado = await usuarioService.UpdateUsuarioAsync(usuarioExistente);
        if (!actualizado)
            return StatusCode(500, "No se pudo actualizar el usuario");

        return NoContent();
    }


    // Eliminar usuario
    [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUsuario(string id)
    {
        var eliminado = await usuarioService.DeleteUsuarioAsync(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}