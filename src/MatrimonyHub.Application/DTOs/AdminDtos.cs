using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.DTOs;

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int PendingNidVerifications { get; set; }
    public int SuccessfulPayments { get; set; }
    public int PendingPayments { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ApprovedSuccessStories { get; set; }
    public List<RecentUserDto> RecentRegistrations { get; set; } = new();
}

public class RecentUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public UserAccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public UserAccountStatus AccountStatus { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int? ProfileId { get; set; }
    public bool HasPendingVerification { get; set; }
}

public class AdminUserDetailDto : AdminUserListDto
{
    public ProfileDetailDto? Profile { get; set; }
    public List<NidVerificationDetailDto> Verifications { get; set; } = new();
    public List<PaymentHistoryDto> Payments { get; set; } = new();
}

public class AdminUpdateUserStatusDto
{
    public int UserId { get; set; }
    public UserAccountStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdminLogDto
{
    public int Id { get; set; }
    public int? AdminUserId { get; set; }
    public string? AdminName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
