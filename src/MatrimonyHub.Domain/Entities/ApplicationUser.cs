using Microsoft.AspNetCore.Identity;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public UserAccountStatus AccountStatus { get; set; } = UserAccountStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual UserProfile? Profile { get; set; }
    public virtual ICollection<NidVerification> NidVerifications { get; set; } = new List<NidVerification>();
    public virtual ICollection<Favorite> FavoritesGiven { get; set; } = new List<Favorite>();
    public virtual ICollection<ContactAccess> ContactAccessesGiven { get; set; } = new List<ContactAccess>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<AdminLog> AdminLogs { get; set; } = new List<AdminLog>();
}
