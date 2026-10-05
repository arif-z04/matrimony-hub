namespace MatrimonyHub.Domain.Entities;

public class LoginAttempt
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public bool Successful { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
