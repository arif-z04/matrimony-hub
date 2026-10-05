using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    // Basic Info
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? HeightCm { get; set; }
    public int? WeightKg { get; set; }
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.NeverMarried;
    public Religion Religion { get; set; } = Religion.Islam;
    public string MotherTongue { get; set; } = "Bengali";
    public string Nationality { get; set; } = "Bangladeshi";

    // Education
    public string? HighestEducation { get; set; }
    public string? Institution { get; set; }
    public string? Subject { get; set; }
    public int? GraduationYear { get; set; }

    // Profession
    public string? Occupation { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? IncomeRange { get; set; }

    // Location
    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "Bangladesh";

    // Lifestyle
    public bool Smoking { get; set; } = false;
    public bool Drinking { get; set; } = false;
    public DietaryPreference DietaryPreference { get; set; } = DietaryPreference.Halal;
    public string? Hobbies { get; set; }
    public string? Interests { get; set; }

    // About
    public string? AboutMe { get; set; }
    public string? FamilyInfo { get; set; }
    public string? PartnerExpectations { get; set; }

    // Status
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Calculated Age helper
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    // Navigations
    public virtual ICollection<ProfilePhoto> Photos { get; set; } = new List<ProfilePhoto>();
    public virtual PartnerPreference? PartnerPreference { get; set; }
    public virtual ICollection<Favorite> FavoritedBy { get; set; } = new List<Favorite>();
    public virtual ICollection<ContactAccess> ContactUnlockedBy { get; set; } = new List<ContactAccess>();
}
