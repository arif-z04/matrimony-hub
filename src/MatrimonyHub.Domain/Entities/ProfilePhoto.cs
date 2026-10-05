namespace MatrimonyHub.Domain.Entities;

public class ProfilePhoto
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public virtual UserProfile UserProfile { get; set; } = null!;

    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
