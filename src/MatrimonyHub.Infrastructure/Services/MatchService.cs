using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

/// <summary>
/// Implements partner searching and an intelligent matching algorithm.
/// Match Scoring Weights (Total Max: 100):
/// 1. Age Range Alignment: Up to 25 points
///    - Candidate age within viewer's preferred [MinAge, MaxAge]: 25 points
///    - Within 3 years of bounds: 15 points
///    - Otherwise: 5 points
/// 2. Religion Alignment: 20 points
///    - Candidate religion matches preferred or viewer religion: 20 points
/// 3. Location / Division Proximity: 15 points
///    - Same division or matches preferred division: 15 points
/// 4. Marital Status Alignment: 10 points
///    - Matches preferred marital status or NeverMarried: 10 points
/// 5. Education Compatibility: 10 points
///    - Matches preferred education or both have higher degrees: 10 points
/// 6. Lifestyle / Dietary Compatibility: 10 points
///    - Matching dietary preferences and non-smoking: 10 points
/// 7. Trust / NID Verification Bonus: 10 points
///    - Profile has approved government NID verification: 10 points
/// </summary>
public class MatchService : IMatchService
{
    private readonly ApplicationDbContext _db;

    public MatchService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProfileCardDto>> SearchMatchesAsync(MatchFilterDto filter, int? currentUserId = null)
    {
        var query = _db.UserProfiles
            .Include(p => p.User)
            .Include(p => p.Photos)
            .Include(p => p.PartnerPreference)
            .Where(p => p.IsActive && !p.User.IsDeleted && p.User.AccountStatus == UserAccountStatus.Active);

        // Exclude current user from their own search results
        if (currentUserId.HasValue)
        {
            query = query.Where(p => p.UserId != currentUserId.Value);
        }

        // Apply filters
        if (filter.Gender.HasValue)
        {
            query = query.Where(p => p.Gender == filter.Gender.Value);
        }

        if (filter.MinAge.HasValue)
        {
            var maxBirthDate = DateTime.UtcNow.AddYears(-filter.MinAge.Value);
            query = query.Where(p => p.DateOfBirth <= maxBirthDate);
        }

        if (filter.MaxAge.HasValue)
        {
            var minBirthDate = DateTime.UtcNow.AddYears(-filter.MaxAge.Value - 1);
            query = query.Where(p => p.DateOfBirth >= minBirthDate);
        }

        if (filter.Religion.HasValue)
        {
            query = query.Where(p => p.Religion == filter.Religion.Value);
        }

        if (filter.MaritalStatus.HasValue)
        {
            query = query.Where(p => p.MaritalStatus == filter.MaritalStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Division))
        {
            query = query.Where(p => p.Division != null && p.Division.ToLower() == filter.Division.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.Education))
        {
            query = query.Where(p => p.HighestEducation != null && p.HighestEducation.ToLower().Contains(filter.Education.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filter.Occupation))
        {
            query = query.Where(p => p.Occupation != null && p.Occupation.ToLower().Contains(filter.Occupation.ToLower()));
        }

        if (filter.VerifiedOnly == true)
        {
            query = query.Where(p => p.IsVerified);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.Trim().ToLower();
            query = query.Where(p =>
                p.FullName.ToLower().Contains(kw) ||
                (p.Occupation != null && p.Occupation.ToLower().Contains(kw)) ||
                (p.HighestEducation != null && p.HighestEducation.ToLower().Contains(kw)) ||
                (p.City != null && p.City.ToLower().Contains(kw)) ||
                (p.District != null && p.District.ToLower().Contains(kw)));
        }

        // Sorting
        query = filter.SortBy switch
        {
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "age_asc" => query.OrderByDescending(p => p.DateOfBirth),
            "age_desc" => query.OrderBy(p => p.DateOfBirth),
            _ => query.OrderByDescending(p => p.IsVerified).ThenByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);
        var page = Math.Max(filter.Page, 1);

        var profiles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get current user's profile for scoring, favorites, and unlocked contacts
        UserProfile? currentUserProfile = null;
        var favoritedProfileIds = new HashSet<int>();
        var unlockedProfileIds = new HashSet<int>();

        if (currentUserId.HasValue)
        {
            currentUserProfile = await _db.UserProfiles
                .Include(p => p.PartnerPreference)
                .FirstOrDefaultAsync(p => p.UserId == currentUserId.Value);

            favoritedProfileIds = (await _db.Favorites
                .Where(f => f.UserId == currentUserId.Value)
                .Select(f => f.FavoriteProfileId)
                .ToListAsync()).ToHashSet();

            unlockedProfileIds = (await _db.ContactAccesses
                .Where(c => c.UserId == currentUserId.Value)
                .Select(c => c.TargetProfileId)
                .ToListAsync()).ToHashSet();
        }

        var items = profiles.Select(p =>
        {
            var primaryPhoto = p.Photos.FirstOrDefault(ph => ph.IsPrimary)?.PhotoUrl
                               ?? p.Photos.FirstOrDefault()?.PhotoUrl;

            int score = currentUserProfile != null ? CalculateMatchScore(currentUserProfile, p) : 75;

            var aboutExcerpt = string.IsNullOrWhiteSpace(p.AboutMe)
                ? null
                : (p.AboutMe.Length > 120 ? p.AboutMe[..117] + "..." : p.AboutMe);

            return new ProfileCardDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = p.FullName,
                Gender = p.Gender,
                Age = p.Age,
                HeightCm = p.HeightCm,
                MaritalStatus = p.MaritalStatus,
                Religion = p.Religion,
                Division = p.Division,
                District = p.District,
                City = p.City,
                HighestEducation = p.HighestEducation,
                Occupation = p.Occupation,
                PrimaryPhotoUrl = primaryPhoto,
                IsVerified = p.IsVerified,
                AboutMeExcerpt = aboutExcerpt,
                MatchScore = score,
                IsFavorited = favoritedProfileIds.Contains(p.Id),
                IsContactUnlocked = unlockedProfileIds.Contains(p.Id)
            };
        }).ToList();

        // If default sorting, also order by match score
        if (filter.SortBy == "match" && currentUserProfile != null)
        {
            items = items.OrderByDescending(i => i.MatchScore).ToList();
        }

        return new PagedResult<ProfileCardDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<ProfileCardDto>> GetSuggestedMatchesAsync(int userId, int count = 6)
    {
        var userProfile = await _db.UserProfiles
            .Include(p => p.PartnerPreference)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (userProfile == null) return new List<ProfileCardDto>();

        // Target opposite gender by default if not explicitly specified
        var targetGender = userProfile.PartnerPreference?.PreferredGender
                           ?? (userProfile.Gender == Gender.Male ? Gender.Female : Gender.Male);

        var filter = new MatchFilterDto
        {
            Gender = targetGender,
            MinAge = userProfile.PartnerPreference?.MinAge,
            MaxAge = userProfile.PartnerPreference?.MaxAge,
            Religion = userProfile.PartnerPreference?.PreferredReligion ?? userProfile.Religion,
            Division = userProfile.PartnerPreference?.PreferredDivision,
            Page = 1,
            PageSize = count,
            SortBy = "match"
        };

        var result = await SearchMatchesAsync(filter, userId);
        return result.Items;
    }

    public int CalculateMatchScore(UserProfile viewer, UserProfile candidate)
    {
        int score = 0;
        var pref = viewer.PartnerPreference;

        // 1. Age alignment (25 pts)
        int candAge = candidate.Age;
        int minAge = pref?.MinAge ?? Math.Max(18, viewer.Age - 5);
        int maxAge = pref?.MaxAge ?? (viewer.Age + 5);

        if (candAge >= minAge && candAge <= maxAge)
        {
            score += 25;
        }
        else if (Math.Abs(candAge - minAge) <= 3 || Math.Abs(candAge - maxAge) <= 3)
        {
            score += 15;
        }
        else
        {
            score += 5;
        }

        // 2. Religion alignment (20 pts)
        var desiredReligion = pref?.PreferredReligion ?? viewer.Religion;
        if (candidate.Religion == desiredReligion)
        {
            score += 20;
        }

        // 3. Location alignment (15 pts)
        var desiredDivision = pref?.PreferredDivision ?? viewer.Division;
        if (!string.IsNullOrWhiteSpace(desiredDivision) && !string.IsNullOrWhiteSpace(candidate.Division)
            && string.Equals(desiredDivision, candidate.Division, StringComparison.OrdinalIgnoreCase))
        {
            score += 15;
        }
        else if (string.Equals(viewer.Country, candidate.Country, StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }

        // 4. Marital Status alignment (10 pts)
        var desiredMarital = pref?.PreferredMaritalStatus ?? MaritalStatus.NeverMarried;
        if (candidate.MaritalStatus == desiredMarital)
        {
            score += 10;
        }

        // 5. Education compatibility (10 pts)
        if (!string.IsNullOrWhiteSpace(pref?.PreferredEducation))
        {
            if (candidate.HighestEducation != null && candidate.HighestEducation.Contains(pref.PreferredEducation, StringComparison.OrdinalIgnoreCase))
                score += 10;
            else
                score += 5;
        }
        else if (!string.IsNullOrWhiteSpace(candidate.HighestEducation) && !string.IsNullOrWhiteSpace(viewer.HighestEducation))
        {
            score += 10;
        }

        // 6. Lifestyle (10 pts)
        if (!candidate.Smoking && !viewer.Smoking) score += 5;
        if (candidate.DietaryPreference == viewer.DietaryPreference) score += 5;

        // 7. Trust Bonus (10 pts)
        if (candidate.IsVerified) score += 10;

        return Math.Clamp(score, 10, 100);
    }
}
