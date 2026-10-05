namespace MatrimonyHub.Domain.Entities;

public class AdminLog
{
    public int Id { get; set; }
    public int? AdminUserId { get; set; }
    public virtual ApplicationUser? AdminUser { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
