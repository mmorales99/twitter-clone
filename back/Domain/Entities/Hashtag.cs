using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Hashtag
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Tag { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public int Contador { get; set; } = 1;
}
