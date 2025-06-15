using AutoRegister;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace application.services;

[Register(ServiceLifetime.Scoped)]
public class JwtTokenService(IConfiguration config)
{
    public string GenerateToken(string userId, string username, string role)
    {
        var claims = new List<Claim>
        {
            new (System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId),
            new (ClaimTypes.Name, username),
            new (ClaimTypes.Role, role),
            new (System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)) { KeyId = userId };
        var encryptingCredentials = new EncryptingCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:EncryptionKey"])),
            SecurityAlgorithms.Aes128KW,
            SecurityAlgorithms.Aes128CbcHmacSha256
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            IssuedAt = DateTime.UtcNow,
            IncludeKeyIdInHeader = true,
            SigningCredentials = creds,
            EncryptingCredentials = encryptingCredentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor); // Esto genera un JWE (token firmado y encriptado)
    }

    public async Task<TokenValidationResult> ValidateEncryptedToken(string token)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var decryptingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:EncryptionKey"]!));

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = signingKey,         // Para validar la firma
            TokenDecryptionKey = decryptingKey,    // Para desencriptar el token
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(15),
            IgnoreTrailingSlashWhenValidatingAudience = true,
            LogValidationExceptions = true,
            SaveSigninToken = true
        };

        var handler = new JsonWebTokenHandler();
        TokenValidationResult result = await handler.ValidateTokenAsync(token, validationParameters);

        // result.IsValid indica si el token es válido y desencriptado correctamente
        return result;
    }
}