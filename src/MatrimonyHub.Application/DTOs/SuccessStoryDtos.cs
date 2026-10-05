using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.DTOs;

public class SuccessStoryDto
{
    public int Id { get; set; }
    public string CoupleNames { get; set; } = string.Empty;
    public string StoryTitle { get; set; } = string.Empty;
    public string StoryDescription { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime MarriageDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public SuccessStoryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SubmittedByName { get; set; }
}

public class CreateSuccessStoryDto
{
    [Required]
    [StringLength(150)]
    public string CoupleNames { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string StoryTitle { get; set; } = string.Empty;

    [Required]
    public string StoryDescription { get; set; } = string.Empty;

    public IFormFile? Photo { get; set; }

    [Required]
    public DateTime MarriageDate { get; set; }

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;
}

public class UpdateSuccessStoryDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string CoupleNames { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string StoryTitle { get; set; } = string.Empty;

    [Required]
    public string StoryDescription { get; set; } = string.Empty;

    public IFormFile? Photo { get; set; }
    public string? ExistingPhotoUrl { get; set; }

    [Required]
    public DateTime MarriageDate { get; set; }

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    public SuccessStoryStatus Status { get; set; }
}
