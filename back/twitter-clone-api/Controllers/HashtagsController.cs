using application.services;
using Microsoft.AspNetCore.Mvc;

namespace twitter_clone_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HashtagsController(
    HashtagService hashtagService
) : ControllerBase
{
    // Obtener lista de hashtags con paginación
    [HttpGet]
    public async Task<IActionResult> GetHashtags(int page = 1, int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page y pageSize deben ser mayores que cero.");

        var hashtags = await hashtagService.GetHashtagsAsync(page, pageSize);
        return Ok(hashtags);
    }

    // Obtener mensajes asociados a un hashtag
    [HttpGet("{nombre}/mensajes")]
    public async Task<IActionResult> GetMensajesPorHashtag(string nombre, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return BadRequest("El nombre del hashtag es obligatorio.");

        var mensajes = await hashtagService.GetMensajesPorHashtagAsync(nombre, page, pageSize);
        return Ok(mensajes);
    }
}