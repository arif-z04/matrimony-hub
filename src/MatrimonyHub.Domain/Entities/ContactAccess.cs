namespace MatrimonyHub.Domain.Entities;

public class ContactAccess
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public int TargetProfileId { get; set; }
    public virtual UserProfile TargetProfile { get; set; } = null!;

    public int PaymentId { get; set; }
    public virtual Payment Payment { get; set; } = null!;

    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}
