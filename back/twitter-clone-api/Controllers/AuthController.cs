using application.services;
using Domain.DTO;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace twitter_clone_api.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthController(
    UsuarioService usuarioService,
    JwtTokenService jwtTokenService,
    RefreshTokenService refreshTokenService,
    IValidator<RegisterDto> crearUsuarioValidator
) : ControllerBase
{
    [EnableRateLimiting("login") , HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await usuarioService.AuthenticateAsync(dto.Username, dto.Password);
        if (usuario == null)
            return Unauthorized();

        var accessToken = jwtTokenService.GenerateToken(usuario.Id, usuario.Username, usuario.Role);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(usuario.Id);

        return Ok(new {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        var storedToken = await refreshTokenService.ValidateAndGetRefreshTokenAsync(dto.RefreshToken);
        if (storedToken == null)
            return Unauthorized();

        var usuario = await usuarioService.GetByIdAsync(storedToken.UserId);
        if (usuario == null)
            return Unauthorized();

        // Revoca el refresh token actual (rotación)
        await refreshTokenService.RevokeRefreshTokenAsync(dto.RefreshToken);

        // Genera nuevos tokens
        var newAccessToken = jwtTokenService.GenerateToken(usuario.Id, usuario.Username, usuario.Role);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(usuario.Id);

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
    {
        await refreshTokenService.RevokeRefreshTokenAsync(dto.RefreshToken);
        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var validationResult = await crearUsuarioValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var usuario = await usuarioService.CrearUsuarioAsync(dto);
        if (usuario is null)
            return BadRequest("No se pudo crear el usuario.");
        
        var token = jwtTokenService.GenerateToken(usuario.Id, usuario.Username, usuario.Role);
        return Ok(new { token });
    }
}