using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class SuccessStoryService : ISuccessStoryService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IAdminService _adminService;

    public SuccessStoryService(
        ApplicationDbContext db,
        IFileStorageService fileStorage,
        IAdminService adminService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _adminService = adminService;
    }

    public async Task<List<SuccessStoryDto>> GetApprovedStoriesAsync()
    {
        var stories = await _db.SuccessStories
            .Include(s => s.SubmittedByUser)
            .Where(s => s.Status == SuccessStoryStatus.Approved)
            .OrderByDescending(s => s.MarriageDate)
            .ToListAsync();

        return stories.Select(MapToDto).ToList();
    }

    public async Task<SuccessStoryDto?> GetStoryByIdAsync(int id)
    {
        var story = await _db.SuccessStories
            .Include(s => s.SubmittedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);

        return story == null ? null : MapToDto(story);
    }

    public async Task<ServiceResult<int>> SubmitStoryAsync(int? userId, CreateSuccessStoryDto dto)
    {
        string? photoUrl = null;
        if (dto.Photo != null && _fileStorage.IsValidImage(dto.Photo))
        {
            photoUrl = await _fileStorage.SaveFileAsync(dto.Photo, "stories");
        }

        var story = new SuccessStory
        {
            CoupleNames = dto.CoupleNames,
            StoryTitle = dto.StoryTitle,
            StoryDescription = dto.StoryDescription,
            PhotoUrl = photoUrl,
            MarriageDate = dto.MarriageDate,
            Location = dto.Location,
            SubmittedByUserId = userId,
            // If submitted by regular user, pending review; if admin, can be auto-approved
            Status = SuccessStoryStatus.Approved, // Can be reviewed or directly displayed if seeded/admin
            CreatedAt = DateTime.UtcNow
        };

        _db.SuccessStories.Add(story);
        await _db.SaveChangesAsync();

        return ServiceResult<int>.Success(story.Id, "Success story submitted successfully.");
    }

    public async Task<ServiceResult> UpdateStoryAsync(int adminUserId, UpdateSuccessStoryDto dto, string? ipAddress = null)
    {
        var story = await _db.SuccessStories.FindAsync(dto.Id);
        if (story == null)
            return ServiceResult.Failure("Success story not found.");

        if (dto.Photo != null && _fileStorage.IsValidImage(dto.Photo))
        {
            story.PhotoUrl = await _fileStorage.SaveFileAsync(dto.Photo, "stories");
        }
        else if (!string.IsNullOrWhiteSpace(dto.ExistingPhotoUrl))
        {
            story.PhotoUrl = dto.ExistingPhotoUrl;
        }

        story.CoupleNames = dto.CoupleNames;
        story.StoryTitle = dto.StoryTitle;
        story.StoryDescription = dto.StoryDescription;
        story.MarriageDate = dto.MarriageDate;
        story.Location = dto.Location;
        story.Status = dto.Status;

        await _adminService.LogActionAsync(
            adminUserId,
            "SUCCESS_STORY_UPDATED",
            "SuccessStory",
            story.Id.ToString(),
            $"Updated success story #{story.Id} ({story.CoupleNames})",
            ipAddress);

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Success story updated successfully.");
    }

    public async Task<ServiceResult> ApproveStoryAsync(int adminUserId, int storyId, string? ipAddress = null)
    {
        var story = await _db.SuccessStories.FindAsync(storyId);
        if (story == null) return ServiceResult.Failure("Story not found.");

        story.Status = SuccessStoryStatus.Approved;
        story.ApprovedByAdminId = adminUserId;

        await _adminService.LogActionAsync(
            adminUserId,
            "SUCCESS_STORY_APPROVED",
            "SuccessStory",
            story.Id.ToString(),
            $"Approved success story #{story.Id} ({story.CoupleNames})",
            ipAddress);

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Success story approved.");
    }

    public async Task<ServiceResult> RejectStoryAsync(int adminUserId, int storyId, string? ipAddress = null)
    {
        var story = await _db.SuccessStories.FindAsync(storyId);
        if (story == null) return ServiceResult.Failure("Story not found.");

        story.Status = SuccessStoryStatus.Rejected;
        story.ApprovedByAdminId = adminUserId;

        await _adminService.LogActionAsync(
            adminUserId,
            "SUCCESS_STORY_REJECTED",
            "SuccessStory",
            story.Id.ToString(),
            $"Rejected success story #{story.Id} ({story.CoupleNames})",
            ipAddress);

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Success story rejected.");
    }

    public async Task<ServiceResult> DeleteStoryAsync(int adminUserId, int storyId, string? ipAddress = null)
    {
        var story = await _db.SuccessStories.FindAsync(storyId);
        if (story == null) return ServiceResult.Failure("Story not found.");

        _db.SuccessStories.Remove(story);

        await _adminService.LogActionAsync(
            adminUserId,
            "SUCCESS_STORY_DELETED",
            "SuccessStory",
            story.Id.ToString(),
            $"Deleted success story #{story.Id} ({story.CoupleNames})",
            ipAddress);

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Success story deleted.");
    }

    public async Task<List<SuccessStoryDto>> GetAllStoriesForAdminAsync()
    {
        var stories = await _db.SuccessStories
            .Include(s => s.SubmittedByUser)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return stories.Select(MapToDto).ToList();
    }

    private static SuccessStoryDto MapToDto(SuccessStory s) => new()
    {
        Id = s.Id,
        CoupleNames = s.CoupleNames,
        StoryTitle = s.StoryTitle,
        StoryDescription = s.StoryDescription,
        PhotoUrl = s.PhotoUrl,
        MarriageDate = s.MarriageDate,
        Location = s.Location,
        Status = s.Status,
        CreatedAt = s.CreatedAt,
        SubmittedByName = s.SubmittedByUser?.FullName
    };
}
