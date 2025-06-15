using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Domain.DTO;
using application.services;

namespace twitter_clone_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MensajesController(
    MensajeService mensajeService,
    IValidator<CrearMensajeDto> crearValidator,
    IValidator<ActualizarMensajeDto> actualizarValidator
) : ControllerBase
{
    // Publicar un mensaje
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CrearMensaje([FromBody] CrearMensajeDto dto)
    {
        var validation = await crearValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var mensaje = await mensajeService.PublicarMensajeAsync(userId, dto.Contenido, dto.ReplyTo);
        return CreatedAtAction(nameof(GetMensaje), new { id = mensaje.Id }, mensaje);
    }

    // Obtener mensaje por id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMensaje(string id)
    {
        var mensaje = await mensajeService.GetMensajePorIdAsync(id);
        if (mensaje is null)
            return NotFound();
        return Ok(mensaje);
    }

    // Obtener mensajes de un usuario
    [HttpGet("usuario/{userId}")]
    public async Task<IActionResult> GetMensajesPorUsuario(string userId, int page = 1, int pageSize = 20)
    {
        var mensajes = await mensajeService.GetMensajesPorUsuarioAsync(userId, page, pageSize);
        return Ok(mensajes);
    }

    // Obtener timeline del usuario autenticado
    [Authorize]
    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline(int page = 1, int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var mensajes = await mensajeService.GetTimelineAsync(userId, page, pageSize);
        return Ok(mensajes);
    }

    // Obtener mensajes por hashtag
    [HttpGet("hashtag/{hashtag}")]
    public async Task<IActionResult> GetMensajesPorHashtag(string hashtag, int page = 1, int pageSize = 20)
    {
        var mensajes = await mensajeService.GetMensajesPorHashtagAsync(hashtag, page, pageSize);
        return Ok(mensajes);
    }

    // Responder a un mensaje
    [Authorize]
    [HttpPost("{id}/reply")]
    public async Task<IActionResult> ResponderMensaje(string id, [FromBody] CrearMensajeDto dto)
    {
        var validation = await crearValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var respuesta = await mensajeService.PublicarMensajeAsync(userId, dto.Contenido, id);
        return CreatedAtAction(nameof(GetMensaje), new { id = respuesta.Id }, respuesta);
    }

    // Actualizar mensaje (solo el autor)
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarMensaje(string id, [FromBody] ActualizarMensajeDto dto)
    {
        var validation = await actualizarValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var actualizado = await mensajeService.ActualizarMensajeAsync(id, userId, dto.Contenido);
        if (!actualizado)
            return Forbid();

        return NoContent();
    }

    // Eliminar mensaje (solo el autor o admin)
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarMensaje(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        // Aquí se asume que el rol "Admin" está correctamente configurado en los claims
        var esAdmin = User.IsInRole("Admin");

        var resultado = await mensajeService.EliminarMensajeAsync(id, userId, esAdmin);
        if (!resultado)
            return Forbid();

        return NoContent();
    }

    // Obtener respuestas a un mensaje
    [HttpGet("{id}/replies")]
    public async Task<IActionResult> GetRespuestas(string id, int page = 1, int pageSize = 20)
    {
        var respuestas = await mensajeService.GetRespuestasAsync(id, page, pageSize);
        return Ok(respuestas);
    }
}
