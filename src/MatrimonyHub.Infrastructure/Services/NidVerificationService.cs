using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class NidVerificationService : INidVerificationService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IAdminService _adminService;

    public NidVerificationService(
        ApplicationDbContext db,
        IFileStorageService fileStorage,
        INotificationService notificationService,
        IEmailService emailService,
        IAdminService adminService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _notificationService = notificationService;
        _emailService = emailService;
        _adminService = adminService;
    }

    public async Task<ServiceResult> SubmitNidVerificationAsync(int userId, NidSubmitDto dto)
    {
        if (!_fileStorage.IsValidDocument(dto.FrontDocument))
            return ServiceResult.Failure("Invalid front document file. JPG, PNG, or PDF up to 10MB accepted.");

        if (dto.BackDocument != null && !_fileStorage.IsValidDocument(dto.BackDocument))
            return ServiceResult.Failure("Invalid back document file. JPG, PNG, or PDF up to 10MB accepted.");

        // Check if there is already an approved verification
        var userProfile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (userProfile == null)
            return ServiceResult.Failure("Profile must be created first.");

        if (userProfile.IsVerified)
            return ServiceResult.Failure("Your identity has already been verified.");

        // Check pending verification
        var pending = await _db.NidVerifications
            .FirstOrDefaultAsync(n => n.UserId == userId && n.Status == NidVerificationStatus.Pending);

        if (pending != null)
            return ServiceResult.Failure("You already have an NID verification pending review.");

        var frontUrl = await _fileStorage.SaveFileAsync(dto.FrontDocument, "verifications");
        string? backUrl = null;
        if (dto.BackDocument != null)
        {
            backUrl = await _fileStorage.SaveFileAsync(dto.BackDocument, "verifications");
        }

        var verification = new NidVerification
        {
            UserId = userId,
            NidNumber = dto.NidNumber.Trim(),
            FrontDocumentUrl = frontUrl,
            BackDocumentUrl = backUrl,
            Status = NidVerificationStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _db.NidVerifications.Add(verification);
        await _db.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            userId,
            "NID Verification Submitted",
            "Your National ID has been submitted for verification. Our compliance team will review your documents shortly.",
            NotificationType.SystemNotification);

        return ServiceResult.Success("NID verification submitted successfully. It will be reviewed by administrators.");
    }

    public async Task<NidVerificationDetailDto?> GetUserVerificationStatusAsync(int userId)
    {
        var verification = await _db.NidVerifications
            .Include(n => n.User)
            .Include(n => n.ReviewedByAdmin)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SubmittedAt)
            .FirstOrDefaultAsync();

        if (verification == null) return null;

        return new NidVerificationDetailDto
        {
            Id = verification.Id,
            UserId = verification.UserId,
            UserName = verification.User.FullName,
            UserEmail = verification.User.Email ?? "",
            // Mask NID number for user privacy
            NidNumber = MaskNid(verification.NidNumber),
            FrontDocumentUrl = verification.FrontDocumentUrl,
            BackDocumentUrl = verification.BackDocumentUrl,
            Status = verification.Status,
            SubmittedAt = verification.SubmittedAt,
            ReviewedAt = verification.ReviewedAt,
            ReviewedByAdminName = verification.ReviewedByAdmin?.FullName,
            RejectionReason = verification.RejectionReason
        };
    }

    public async Task<List<NidVerificationDetailDto>> GetPendingVerificationsAsync()
    {
        var list = await _db.NidVerifications
            .Include(n => n.User)
            .Where(n => n.Status == NidVerificationStatus.Pending)
            .OrderBy(n => n.SubmittedAt)
            .ToListAsync();

        return list.Select(n => new NidVerificationDetailDto
        {
            Id = n.Id,
            UserId = n.UserId,
            UserName = n.User.FullName,
            UserEmail = n.User.Email ?? "",
            NidNumber = n.NidNumber, // Visible to admin for document cross-checking
            FrontDocumentUrl = n.FrontDocumentUrl,
            BackDocumentUrl = n.BackDocumentUrl,
            Status = n.Status,
            SubmittedAt = n.SubmittedAt
        }).ToList();
    }

    public async Task<ServiceResult> ReviewVerificationAsync(int adminUserId, NidReviewDto dto, string? ipAddress = null)
    {
        var verification = await _db.NidVerifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == dto.VerificationId);

        if (verification == null)
            return ServiceResult.Failure("Verification request not found.");

        if (verification.Status != NidVerificationStatus.Pending)
            return ServiceResult.Failure("This verification request has already been reviewed.");

        // CRITICAL BUSINESS RULE: Admin cannot approve their own NID!
        if (verification.UserId == adminUserId)
        {
            return ServiceResult.Failure("Security violation: Administrators cannot review or approve their own identity verification.");
        }

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == verification.UserId);

        verification.ReviewedAt = DateTime.UtcNow;
        verification.ReviewedByAdminId = adminUserId;

        if (dto.Status == NidVerificationStatus.Approved)
        {
            verification.Status = NidVerificationStatus.Approved;
            verification.RejectionReason = null;
            if (profile != null)
            {
                profile.IsVerified = true;
            }

            await _adminService.LogActionAsync(
                adminUserId,
                "NID_APPROVED",
                "NidVerification",
                verification.Id.ToString(),
                $"Approved NID verification for User {verification.UserId} ({verification.User.FullName})",
                ipAddress);

            await _notificationService.CreateNotificationAsync(
                verification.UserId,
                "NID Verification Approved",
                "Congratulations! Your National ID verification was approved. You now have a verified badge on your profile.",
                NotificationType.VerificationApproved);

            if (verification.User.Email != null)
            {
                await _emailService.SendVerificationStatusEmailAsync(verification.User.Email, verification.User.FullName, true, null);
            }
        }
        else if (dto.Status == NidVerificationStatus.Rejected)
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                return ServiceResult.Failure("A rejection reason is required when rejecting a verification request.");

            verification.Status = NidVerificationStatus.Rejected;
            verification.RejectionReason = dto.RejectionReason;
            if (profile != null)
            {
                profile.IsVerified = false;
            }

            await _adminService.LogActionAsync(
                adminUserId,
                "NID_REJECTED",
                "NidVerification",
                verification.Id.ToString(),
                $"Rejected NID verification for User {verification.UserId} ({verification.User.FullName}). Reason: {dto.RejectionReason}",
                ipAddress);

            await _notificationService.CreateNotificationAsync(
                verification.UserId,
                "NID Verification Rejected",
                $"Your NID verification could not be approved. Reason: {dto.RejectionReason}",
                NotificationType.VerificationRejected);

            if (verification.User.Email != null)
            {
                await _emailService.SendVerificationStatusEmailAsync(verification.User.Email, verification.User.FullName, false, dto.RejectionReason);
            }
        }

        await _db.SaveChangesAsync();
        return ServiceResult.Success("Verification review saved successfully.");
    }

    private static string MaskNid(string nid)
    {
        if (string.IsNullOrEmpty(nid) || nid.Length < 6) return "******";
        return nid[..2] + new string('*', nid.Length - 4) + nid[^2..];
    }
}
