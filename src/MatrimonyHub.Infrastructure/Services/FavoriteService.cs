using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public FavoriteService(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<ServiceResult> ToggleFavoriteAsync(int userId, int targetProfileId)
    {
        var targetProfile = await _db.UserProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == targetProfileId);

        if (targetProfile == null)
            return ServiceResult.Failure("Target profile does not exist.");

        // Rule: cannot favorite yourself
        if (targetProfile.UserId == userId)
            return ServiceResult.Failure("You cannot add your own profile to favorites.");

        var existing = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FavoriteProfileId == targetProfileId);

        if (existing != null)
        {
            _db.Favorites.Remove(existing);
            await _db.SaveChangesAsync();
            return ServiceResult.Success("Removed from favorites.");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            FavoriteProfileId = targetProfileId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Favorites.Add(favorite);
        await _db.SaveChangesAsync();

        // Notify target profile user
        var currentUser = await _db.Users.FindAsync(userId);
        if (currentUser != null)
        {
            await _notificationService.CreateNotificationAsync(
                targetProfile.UserId,
                "New Favorite",
                $"{currentUser.FullName} added your profile to their favorites.",
                NotificationType.FavoriteAdded,
                userId.ToString());
        }

        return ServiceResult.Success("Added to favorites.");
    }

    public async Task<List<ProfileCardDto>> GetUserFavoritesAsync(int userId)
    {
        var favorites = await _db.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.FavoriteProfile)
                .ThenInclude(p => p.User)
            .Include(f => f.FavoriteProfile)
                .ThenInclude(p => p.Photos)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var unlockedIds = (await _db.ContactAccesses
            .Where(c => c.UserId == userId)
            .Select(c => c.TargetProfileId)
            .ToListAsync()).ToHashSet();

        return favorites.Select(f =>
        {
            var p = f.FavoriteProfile;
            var primaryPhoto = p.Photos.FirstOrDefault(ph => ph.IsPrimary)?.PhotoUrl
                               ?? p.Photos.FirstOrDefault()?.PhotoUrl;

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
                AboutMeExcerpt = p.AboutMe,
                IsFavorited = true,
                IsContactUnlocked = unlockedIds.Contains(p.Id)
            };
        }).ToList();
    }

    public Task<bool> IsFavoriteAsync(int userId, int targetProfileId)
    {
        return _db.Favorites.AnyAsync(f => f.UserId == userId && f.FavoriteProfileId == targetProfileId);
    }
}
