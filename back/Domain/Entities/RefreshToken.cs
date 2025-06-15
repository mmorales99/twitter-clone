namespace Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } // Valor único y aleatorio
    public string UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
}
