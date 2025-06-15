using Domain.DTO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Bio { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    [BsonElement("Followers")]
    public List<string> FollowerIds { get; set; } = [];  // Usuarios que me siguen

    [BsonElement("Following")]
    public List<string> FollowingIds { get; set; } = []; // Usuarios que sigo
    public string Role { get; set; }
    public string Biografia { get; set; }
    public string Nombre { get; set; }

    public static Usuario CreateFromDTO(RegisterDto register) 
    {
        return new Usuario() 
        {
            Username = register.Username,
            Email = register.Email,
        };
    }
}