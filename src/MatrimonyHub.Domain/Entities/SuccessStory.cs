using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class SuccessStory
{
    public int Id { get; set; }
    public string CoupleNames { get; set; } = string.Empty;
    public string StoryTitle { get; set; } = string.Empty;
    public string StoryDescription { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime MarriageDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public SuccessStoryStatus Status { get; set; } = SuccessStoryStatus.Approved;

    public int? SubmittedByUserId { get; set; }
    public virtual ApplicationUser? SubmittedByUser { get; set; }

    public int? ApprovedByAdminId { get; set; }
    public virtual ApplicationUser? ApprovedByAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
