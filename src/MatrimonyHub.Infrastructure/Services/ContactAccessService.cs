using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.PaymentGateways;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class ContactAccessService : IContactAccessService
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentGatewayOptions _options;

    public ContactAccessService(ApplicationDbContext db, IOptions<PaymentGatewayOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<bool> HasAccessAsync(int userId, int targetProfileId)
    {
        // 1. Is user owner of target profile?
        var profile = await _db.UserProfiles.FindAsync(targetProfileId);
        if (profile == null) return false;
        if (profile.UserId == userId) return true;

        // An unverified user cannot view contact info of others
        var viewerProfile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (viewerProfile == null || !viewerProfile.IsVerified)
        {
            return false;
        }

        // 2. Check existing access record in DB
        return await _db.ContactAccesses.AnyAsync(c => c.UserId == userId && c.TargetProfileId == targetProfileId);
    }

    public async Task<ServiceResult> UnlockContactAsync(int userId, int targetProfileId, int paymentId)
    {
        var viewerProfile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (viewerProfile == null || !viewerProfile.IsVerified)
        {
            return ServiceResult.Failure("Identity Verification Required: You must verify your National ID (NID) before you can unlock contact details.");
        }

        // Verify payment is successful and matches target profile
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);
        if (payment == null)
            return ServiceResult.Failure("Payment record not found.");

        if (payment.Status != PaymentStatus.Successful)
            return ServiceResult.Failure("Cannot grant contact access without confirmed successful payment.");

        if (payment.TargetProfileId != targetProfileId)
            return ServiceResult.Failure("Payment was not intended for this profile.");

        // Check if access already exists (idempotency)
        var exists = await _db.ContactAccesses.AnyAsync(c => c.UserId == userId && c.TargetProfileId == targetProfileId);
        if (exists)
        {
            return ServiceResult.Success("Contact details already unlocked.");
        }

        var access = new ContactAccess
        {
            UserId = userId,
            TargetProfileId = targetProfileId,
            PaymentId = paymentId,
            UnlockedAt = DateTime.UtcNow
        };

        _db.ContactAccesses.Add(access);
        await _db.SaveChangesAsync();

        return ServiceResult.Success("Contact details unlocked successfully.");
    }

    public async Task<List<ProfileCardDto>> GetUnlockedProfilesAsync(int userId)
    {
        var viewerProfile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (viewerProfile == null || !viewerProfile.IsVerified)
        {
            return new List<ProfileCardDto>();
        }

        var accesses = await _db.ContactAccesses
            .Where(c => c.UserId == userId)
            .Include(c => c.TargetProfile)
                .ThenInclude(p => p.User)
            .Include(c => c.TargetProfile)
                .ThenInclude(p => p.Photos)
            .OrderByDescending(c => c.UnlockedAt)
            .ToListAsync();

        return accesses.Select(c =>
        {
            var p = c.TargetProfile;
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
                IsFavorited = false,
                IsContactUnlocked = true
            };
        }).ToList();
    }

    public Task<decimal> GetContactUnlockFeeAsync()
    {
        return Task.FromResult(_options.ContactUnlockFee > 0 ? _options.ContactUnlockFee : 500.00m);
    }
}
