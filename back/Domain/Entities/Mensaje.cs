using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Mensaje
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<string> Hashtags { get; set; } = new();
    public List<string> ImageIds { get; set; } = new();
    public List<string> VideoIds { get; set; } = new();
    public List<string> Likes { get; set; } = new();
    public string? ReplyTo { get; set; }
}
