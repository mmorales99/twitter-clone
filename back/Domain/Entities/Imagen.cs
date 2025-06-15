using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Imagen
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Url { get; set; } = default!;
    public string UploadedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
