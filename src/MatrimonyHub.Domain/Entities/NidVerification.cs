using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class NidVerification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public string NidNumber { get; set; } = string.Empty;
    public string FrontDocumentUrl { get; set; } = string.Empty;
    public string? BackDocumentUrl { get; set; }
    public NidVerificationStatus Status { get; set; } = NidVerificationStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public virtual ApplicationUser? ReviewedByAdmin { get; set; }
    public string? RejectionReason { get; set; }
}
