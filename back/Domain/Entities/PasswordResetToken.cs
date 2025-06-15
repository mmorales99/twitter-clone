namespace Domain.Entities;

public class PasswordResetToken
{
    public string Token { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
}
