using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.DTOs;

public class NidSubmitDto
{
    [Required(ErrorMessage = "NID number is required")]
    [RegularExpression(@"^(\d{10}|\d{13}|\d{17})$", ErrorMessage = "NID must be 10, 13, or 17 digits")]
    public string NidNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Front side document is required")]
    public IFormFile FrontDocument { get; set; } = null!;

    public IFormFile? BackDocument { get; set; }
}

public class NidVerificationDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string NidNumber { get; set; } = string.Empty; // Protected, only visible to admin
    public string FrontDocumentUrl { get; set; } = string.Empty;
    public string? BackDocumentUrl { get; set; }
    public NidVerificationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByAdminName { get; set; }
    public string? RejectionReason { get; set; }
}

public class NidReviewDto
{
    [Required]
    public int VerificationId { get; set; }

    [Required]
    public NidVerificationStatus Status { get; set; }

    public string? RejectionReason { get; set; }
}
