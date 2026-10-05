using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.DTOs;

public class ProfileCardDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int? HeightCm { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public Religion Religion { get; set; }
    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? HighestEducation { get; set; }
    public string? Occupation { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public bool IsVerified { get; set; }
    public string? AboutMeExcerpt { get; set; }
    public int MatchScore { get; set; }
    public bool IsFavorited { get; set; }
    public bool IsContactUnlocked { get; set; }
}

public class ProfileDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public int? HeightCm { get; set; }
    public int? WeightKg { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public Religion Religion { get; set; }
    public string MotherTongue { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;

    public string? HighestEducation { get; set; }
    public string? Institution { get; set; }
    public string? Subject { get; set; }
    public int? GraduationYear { get; set; }

    public string? Occupation { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? IncomeRange { get; set; }

    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "Bangladesh";

    public bool Smoking { get; set; }
    public bool Drinking { get; set; }
    public DietaryPreference DietaryPreference { get; set; }
    public string? Hobbies { get; set; }
    public string? Interests { get; set; }

    public string? AboutMe { get; set; }
    public string? FamilyInfo { get; set; }
    public string? PartnerExpectations { get; set; }

    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<PhotoItemDto> Photos { get; set; } = new();
    public PartnerPreferenceDto? PartnerPreference { get; set; }

    public bool IsFavorited { get; set; }
    public bool IsContactUnlocked { get; set; }
    public bool IsOwner { get; set; }

    // Privacy protected fields - ONLY populated if unlocked or owner!
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactAddress { get; set; }
}

public class PhotoItemDto
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class PartnerPreferenceDto
{
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
}

public class UpdateProfileDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? HeightCm { get; set; }
    public int? WeightKg { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public Religion Religion { get; set; }
    public string MotherTongue { get; set; } = "Bengali";
    public string Nationality { get; set; } = "Bangladeshi";

    public string? HighestEducation { get; set; }
    public string? Institution { get; set; }
    public string? Subject { get; set; }
    public int? GraduationYear { get; set; }

    public string? Occupation { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? IncomeRange { get; set; }

    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "Bangladesh";

    public bool Smoking { get; set; }
    public bool Drinking { get; set; }
    public DietaryPreference DietaryPreference { get; set; }
    public string? Hobbies { get; set; }
    public string? Interests { get; set; }

    public string? AboutMe { get; set; }
    public string? FamilyInfo { get; set; }
    public string? PartnerExpectations { get; set; }
}

public class UpdatePreferencesDto
{
    [Range(18, 80)]
    public int? MinAge { get; set; }

    [Range(18, 80)]
    public int? MaxAge { get; set; }

    public Gender? PreferredGender { get; set; }
    public Religion? PreferredReligion { get; set; }
    public MaritalStatus? PreferredMaritalStatus { get; set; }
    public string? PreferredEducation { get; set; }
    public string? PreferredOccupation { get; set; }
    public string? PreferredDivision { get; set; }
    public DietaryPreference? DietaryPreference { get; set; }
    public string? Notes { get; set; }
}

public class UploadPhotoDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
    public bool SetAsPrimary { get; set; }
}
