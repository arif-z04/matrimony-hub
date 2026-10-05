using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public int? TargetProfileId { get; set; }
    public virtual UserProfile? TargetProfile { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public PaymentPurpose PaymentPurpose { get; set; } = PaymentPurpose.ContactUnlock;
    public string TransactionId { get; set; } = string.Empty;
    public PaymentGateway Gateway { get; set; } = PaymentGateway.Sandbox;
    public string? GatewayTransactionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    public virtual ContactAccess? ContactAccess { get; set; }
}
