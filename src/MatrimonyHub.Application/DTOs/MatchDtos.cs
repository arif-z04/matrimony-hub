using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.DTOs;

public class MatchFilterDto
{
    public Gender? Gender { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Religion? Religion { get; set; }
    public MaritalStatus? MaritalStatus { get; set; }
    public string? Division { get; set; }
    public string? Education { get; set; }
    public string? Occupation { get; set; }
    public bool? VerifiedOnly { get; set; }
    public string? Keyword { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string? SortBy { get; set; } = "match"; // match, newest, age_asc, age_desc
}
