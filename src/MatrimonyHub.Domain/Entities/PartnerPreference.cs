using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class PartnerPreference
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public virtual UserProfile UserProfile { get; set; } = null!;

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Gender? PreferredGender { get; set; }
    public Religion? PreferredReligion { get; set; }
    public MaritalStatus? PreferredMaritalStatus { get; set; }
    public string? PreferredEducation { get; set; }
    public string? PreferredOccupation { get; set; }
    public string? PreferredDivision { get; set; }
    public DietaryPreference? DietaryPreference { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
