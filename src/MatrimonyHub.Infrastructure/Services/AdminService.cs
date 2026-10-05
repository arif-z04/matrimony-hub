using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;

    public AdminService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalUsers = await _db.Users.CountAsync(u => !u.IsDeleted);
        var activeUsers = await _db.Users.CountAsync(u => !u.IsDeleted && u.AccountStatus == UserAccountStatus.Active);
        var verifiedUsers = await _db.UserProfiles.CountAsync(p => p.IsVerified && p.IsActive);
        var pendingNid = await _db.NidVerifications.CountAsync(n => n.Status == NidVerificationStatus.Pending);
        var successfulPayments = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.Successful);
        var pendingPayments = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);
        var totalTransactions = await _db.PaymentTransactions.CountAsync();
        var totalRevenue = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Successful)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var approvedStories = await _db.SuccessStories.CountAsync(s => s.Status == SuccessStoryStatus.Approved);

        var recentUsers = await _db.Users
            .Include(u => u.Profile)
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .Take(6)
            .Select(u => new RecentUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                IsVerified = u.Profile != null && u.Profile.IsVerified,
                Status = u.AccountStatus,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new AdminDashboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            VerifiedUsers = verifiedUsers,
            PendingNidVerifications = pendingNid,
            SuccessfulPayments = successfulPayments,
            PendingPayments = pendingPayments,
            TotalTransactions = totalTransactions,
            TotalRevenue = totalRevenue,
            ApprovedSuccessStories = approvedStories,
            RecentRegistrations = recentUsers
        };
    }

    public async Task<PagedResult<AdminUserListDto>> GetUsersAsync(int page, int pageSize, string? search = null, UserAccountStatus? status = null)
    {
        var query = _db.Users
            .Include(u => u.Profile)
            .Include(u => u.NidVerifications)
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(u => u.AccountStatus == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || (u.Email != null && u.Email.ToLower().Contains(s)) || (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
        }

        var totalCount = await query.CountAsync();
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = users.Select(u => new AdminUserListDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? "",
            PhoneNumber = u.PhoneNumber ?? "",
            AccountStatus = u.AccountStatus,
            IsVerified = u.Profile != null && u.Profile.IsVerified,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            ProfileId = u.Profile?.Id,
            HasPendingVerification = u.NidVerifications.Any(n => n.Status == NidVerificationStatus.Pending)
        }).ToList();

        return new PagedResult<AdminUserListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDetailDto?> GetUserDetailAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
                .ThenInclude(p => p!.Photos)
            .Include(u => u.Profile)
                .ThenInclude(p => p!.PartnerPreference)
            .Include(u => u.NidVerifications)
                .ThenInclude(n => n.ReviewedByAdmin)
            .Include(u => u.Payments)
                .ThenInclude(p => p.TargetProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var verifications = user.NidVerifications.Select(n => new NidVerificationDetailDto
        {
            Id = n.Id,
            UserId = n.UserId,
            UserName = user.FullName,
            UserEmail = user.Email ?? "",
            NidNumber = n.NidNumber,
            FrontDocumentUrl = n.FrontDocumentUrl,
            BackDocumentUrl = n.BackDocumentUrl,
            Status = n.Status,
            SubmittedAt = n.SubmittedAt,
            ReviewedAt = n.ReviewedAt,
            ReviewedByAdminName = n.ReviewedByAdmin?.FullName,
            RejectionReason = n.RejectionReason
        }).OrderByDescending(n => n.SubmittedAt).ToList();

        var payments = user.Payments.Select(p => new PaymentHistoryDto
        {
            Id = p.Id,
            TransactionId = p.TransactionId,
            TargetProfileId = p.TargetProfileId,
            TargetProfileName = p.TargetProfile?.FullName,
            Amount = p.Amount,
            Currency = p.Currency,
            Purpose = p.PaymentPurpose,
            Gateway = p.Gateway,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt
        }).OrderByDescending(p => p.CreatedAt).ToList();

        return new AdminUserDetailDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            AccountStatus = user.AccountStatus,
            IsVerified = user.Profile != null && user.Profile.IsVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            ProfileId = user.Profile?.Id,
            HasPendingVerification = user.NidVerifications.Any(n => n.Status == NidVerificationStatus.Pending),
            Verifications = verifications,
            Payments = payments
        };
    }

    public async Task<ServiceResult> UpdateUserStatusAsync(int adminUserId, AdminUpdateUserStatusDto dto, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _db.Users.FindAsync(dto.UserId);
        if (user == null) return ServiceResult.Failure("User not found.");

        if (user.Id == adminUserId)
            return ServiceResult.Failure("Cannot modify your own administrator account status.");

        var oldStatus = user.AccountStatus;
        user.AccountStatus = dto.Status;

        await LogActionAsync(
            adminUserId,
            "USER_STATUS_CHANGE",
            "ApplicationUser",
            user.Id.ToString(),
            $"Changed status for user {user.Id} ({user.FullName}) from {oldStatus} to {dto.Status}. Reason: {dto.Reason}",
            ipAddress,
            userAgent);

        await _db.SaveChangesAsync();
        return ServiceResult.Success($"User account status changed to {dto.Status}.");
    }

    public async Task<PagedResult<AdminLogDto>> GetLogsAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.AdminLogs
            .Include(l => l.AdminUser)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = logs.Select(l => new AdminLogDto
        {
            Id = l.Id,
            AdminUserId = l.AdminUserId,
            AdminName = l.AdminUser?.FullName,
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            Description = l.Description,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            CreatedAt = l.CreatedAt
        }).ToList();

        return new PagedResult<AdminLogDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task LogActionAsync(int? adminUserId, string action, string entityType, string? entityId, string description, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var log = new AdminLog
            {
                AdminUserId = adminUserId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _db.AdminLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Do not fail primary flow on logging error
        }
    }
}
