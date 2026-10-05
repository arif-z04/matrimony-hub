using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IContactAccessService _contactAccessService;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        ApplicationDbContext db,
        IFileStorageService fileStorage,
        IContactAccessService contactAccessService,
        ILogger<ProfileService> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _contactAccessService = contactAccessService;
        _logger = logger;
    }

    public async Task<ServiceResult<ProfileDetailDto>> GetProfileByUserIdAsync(int targetUserId, int? requestingUserId = null)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.User)
            .Include(p => p.Photos)
            .Include(p => p.PartnerPreference)
            .FirstOrDefaultAsync(p => p.UserId == targetUserId && p.IsActive);

        if (profile == null)
            return ServiceResult<ProfileDetailDto>.Failure("Matrimonial profile not found.");

        return await BuildProfileDetailDtoAsync(profile, requestingUserId);
    }

    public async Task<ServiceResult<ProfileDetailDto>> GetProfileByIdAsync(int profileId, int? requestingUserId = null)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.User)
            .Include(p => p.Photos)
            .Include(p => p.PartnerPreference)
            .FirstOrDefaultAsync(p => p.Id == profileId && p.IsActive);

        if (profile == null)
            return ServiceResult<ProfileDetailDto>.Failure("Matrimonial profile not found.");

        return await BuildProfileDetailDtoAsync(profile, requestingUserId);
    }

    private async Task<ServiceResult<ProfileDetailDto>> BuildProfileDetailDtoAsync(UserProfile profile, int? requestingUserId)
    {
        bool isOwner = requestingUserId.HasValue && requestingUserId.Value == profile.UserId;
        bool isUnlocked = false;
        bool isFavorited = false;

        if (requestingUserId.HasValue && !isOwner)
        {
            isUnlocked = await _contactAccessService.HasAccessAsync(requestingUserId.Value, profile.Id);
            isFavorited = await _db.Favorites.AnyAsync(f => f.UserId == requestingUserId.Value && f.FavoriteProfileId == profile.Id);

            // Record profile viewed notification if viewer is different
            try
            {
                var viewer = await _db.Users.FindAsync(requestingUserId.Value);
                if (viewer != null)
                {
                    _db.Notifications.Add(new Notification
                    {
                        UserId = profile.UserId,
                        Title = "Profile Viewed",
                        Message = $"{viewer.FullName} recently viewed your matrimonial profile.",
                        Type = Domain.Enums.NotificationType.ProfileViewed,
                        RelatedEntityId = requestingUserId.Value.ToString(),
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not log profile view notification");
            }
        }

        var dto = new ProfileDetailDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            Gender = profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            Age = profile.Age,
            HeightCm = profile.HeightCm,
            WeightKg = profile.WeightKg,
            MaritalStatus = profile.MaritalStatus,
            Religion = profile.Religion,
            MotherTongue = profile.MotherTongue,
            Nationality = profile.Nationality,
            HighestEducation = profile.HighestEducation,
            Institution = profile.Institution,
            Subject = profile.Subject,
            GraduationYear = profile.GraduationYear,
            Occupation = profile.Occupation,
            Company = profile.Company,
            JobTitle = profile.JobTitle,
            IncomeRange = profile.IncomeRange,
            Division = profile.Division,
            District = profile.District,
            City = profile.City,
            Country = profile.Country,
            Smoking = profile.Smoking,
            Drinking = profile.Drinking,
            DietaryPreference = profile.DietaryPreference,
            Hobbies = profile.Hobbies,
            Interests = profile.Interests,
            AboutMe = profile.AboutMe,
            FamilyInfo = profile.FamilyInfo,
            PartnerExpectations = profile.PartnerExpectations,
            IsVerified = profile.IsVerified,
            CreatedAt = profile.CreatedAt,
            IsOwner = isOwner,
            IsContactUnlocked = isUnlocked,
            IsFavorited = isFavorited,
            Photos = profile.Photos.Select(p => new PhotoItemDto
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                IsPrimary = p.IsPrimary
            }).OrderByDescending(p => p.IsPrimary).ToList(),
            PartnerPreference = profile.PartnerPreference == null ? null : new PartnerPreferenceDto
            {
                MinAge = profile.PartnerPreference.MinAge,
                MaxAge = profile.PartnerPreference.MaxAge,
                PreferredGender = profile.PartnerPreference.PreferredGender,
                PreferredReligion = profile.PartnerPreference.PreferredReligion,
                PreferredMaritalStatus = profile.PartnerPreference.PreferredMaritalStatus,
                PreferredEducation = profile.PartnerPreference.PreferredEducation,
                PreferredOccupation = profile.PartnerPreference.PreferredOccupation,
                PreferredDivision = profile.PartnerPreference.PreferredDivision,
                DietaryPreference = profile.PartnerPreference.DietaryPreference,
                Notes = profile.PartnerPreference.Notes
            }
        };

        // PRIVACY ENFORCEMENT: Contact details are ONLY populated if unlocked or owner!
        if (isOwner || isUnlocked)
        {
            dto.ContactPhone = profile.User?.PhoneNumber;
            dto.ContactEmail = profile.User?.Email;
            dto.ContactAddress = $"{profile.City}, {profile.District}, {profile.Division}, {profile.Country}";
        }

        return ServiceResult<ProfileDetailDto>.Success(dto);
    }

    public async Task<ServiceResult> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return ServiceResult.Failure("Profile not found.");

        profile.FullName = dto.FullName;
        profile.Gender = dto.Gender;
        profile.DateOfBirth = dto.DateOfBirth;
        profile.HeightCm = dto.HeightCm;
        profile.WeightKg = dto.WeightKg;
        profile.MaritalStatus = dto.MaritalStatus;
        profile.Religion = dto.Religion;
        profile.MotherTongue = dto.MotherTongue;
        profile.Nationality = dto.Nationality;
        profile.HighestEducation = dto.HighestEducation;
        profile.Institution = dto.Institution;
        profile.Subject = dto.Subject;
        profile.GraduationYear = dto.GraduationYear;
        profile.Occupation = dto.Occupation;
        profile.Company = dto.Company;
        profile.JobTitle = dto.JobTitle;
        profile.IncomeRange = dto.IncomeRange;
        profile.Division = dto.Division;
        profile.District = dto.District;
        profile.City = dto.City;
        profile.Country = dto.Country;
        profile.Smoking = dto.Smoking;
        profile.Drinking = dto.Drinking;
        profile.DietaryPreference = dto.DietaryPreference;
        profile.Hobbies = dto.Hobbies;
        profile.Interests = dto.Interests;
        profile.AboutMe = dto.AboutMe;
        profile.FamilyInfo = dto.FamilyInfo;
        profile.PartnerExpectations = dto.PartnerExpectations;
        profile.UpdatedAt = DateTime.UtcNow;

        if (profile.User != null)
        {
            profile.User.FullName = dto.FullName;
        }

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Profile updated successfully.");
    }

    public async Task<ServiceResult> UpdatePreferencesAsync(int userId, UpdatePreferencesDto dto)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.PartnerPreference)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return ServiceResult.Failure("Profile not found.");

        if (profile.PartnerPreference == null)
        {
            profile.PartnerPreference = new PartnerPreference
            {
                UserProfileId = profile.Id,
                CreatedAt = DateTime.UtcNow
            };
            _db.PartnerPreferences.Add(profile.PartnerPreference);
        }

        profile.PartnerPreference.MinAge = dto.MinAge;
        profile.PartnerPreference.MaxAge = dto.MaxAge;
        profile.PartnerPreference.PreferredGender = dto.PreferredGender;
        profile.PartnerPreference.PreferredReligion = dto.PreferredReligion;
        profile.PartnerPreference.PreferredMaritalStatus = dto.PreferredMaritalStatus;
        profile.PartnerPreference.PreferredEducation = dto.PreferredEducation;
        profile.PartnerPreference.PreferredOccupation = dto.PreferredOccupation;
        profile.PartnerPreference.PreferredDivision = dto.PreferredDivision;
        profile.PartnerPreference.DietaryPreference = dto.DietaryPreference;
        profile.PartnerPreference.Notes = dto.Notes;
        profile.PartnerPreference.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Partner preferences updated successfully.");
    }

    public async Task<ServiceResult<string>> UploadPhotoAsync(int userId, UploadPhotoDto dto)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return ServiceResult<string>.Failure("Profile not found.");

        if (!_fileStorage.IsValidImage(dto.File))
            return ServiceResult<string>.Failure("Invalid image format or size exceeds 5MB. Only JPG, PNG, WEBP allowed.");

        if (profile.Photos.Count >= 6)
            return ServiceResult<string>.Failure("You can upload a maximum of 6 photos.");

        var photoUrl = await _fileStorage.SaveFileAsync(dto.File, "profiles");

        bool shouldBePrimary = dto.SetAsPrimary || !profile.Photos.Any(p => p.IsPrimary);
        if (shouldBePrimary)
        {
            foreach (var existing in profile.Photos)
            {
                existing.IsPrimary = false;
            }
        }

        var photo = new ProfilePhoto
        {
            UserProfileId = profile.Id,
            PhotoUrl = photoUrl,
            IsPrimary = shouldBePrimary,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProfilePhotos.Add(photo);
        await _db.SaveChangesAsync();

        return ServiceResult<string>.Success(photoUrl, "Photo uploaded successfully.");
    }

    public async Task<ServiceResult> SetPrimaryPhotoAsync(int userId, int photoId)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return ServiceResult.Failure("Profile not found.");

        var photo = profile.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return ServiceResult.Failure("Photo not found.");

        foreach (var p in profile.Photos)
        {
            p.IsPrimary = (p.Id == photoId);
        }

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Primary photo updated.");
    }

    public async Task<ServiceResult> DeletePhotoAsync(int userId, int photoId)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return ServiceResult.Failure("Profile not found.");

        var photo = profile.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return ServiceResult.Failure("Photo not found.");

        _fileStorage.DeleteFile(photo.PhotoUrl);
        _db.ProfilePhotos.Remove(photo);

        if (photo.IsPrimary)
        {
            var next = profile.Photos.FirstOrDefault(p => p.Id != photoId);
            if (next != null) next.IsPrimary = true;
        }

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Photo deleted.");
    }

    public async Task<int> CalculateProfileCompletionAsync(int userId)
    {
        var profile = await _db.UserProfiles
            .Include(p => p.Photos)
            .Include(p => p.PartnerPreference)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return 0;

        int score = 0;
        // Basic Info: 25%
        if (!string.IsNullOrWhiteSpace(profile.FullName) && profile.DateOfBirth != default) score += 15;
        if (profile.HeightCm.HasValue) score += 5;
        if (!string.IsNullOrWhiteSpace(profile.Religion.ToString())) score += 5;

        // Photo: 20%
        if (profile.Photos.Any()) score += 20;

        // Education & Profession: 20%
        if (!string.IsNullOrWhiteSpace(profile.HighestEducation)) score += 10;
        if (!string.IsNullOrWhiteSpace(profile.Occupation)) score += 10;

        // Location: 15%
        if (!string.IsNullOrWhiteSpace(profile.Division)) score += 10;
        if (!string.IsNullOrWhiteSpace(profile.District) || !string.IsNullOrWhiteSpace(profile.City)) score += 5;

        // About & Lifestyle: 10%
        if (!string.IsNullOrWhiteSpace(profile.AboutMe)) score += 10;

        // Partner Preferences: 10%
        if (profile.PartnerPreference != null && (profile.PartnerPreference.MinAge.HasValue || profile.PartnerPreference.PreferredReligion.HasValue)) score += 10;

        return Math.Min(100, score);
    }
}
