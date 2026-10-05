using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface IMatchService
{
    Task<PagedResult<ProfileCardDto>> SearchMatchesAsync(MatchFilterDto filter, int? currentUserId = null);
    Task<List<ProfileCardDto>> GetSuggestedMatchesAsync(int userId, int count = 6);
    int CalculateMatchScore(Domain.Entities.UserProfile viewer, Domain.Entities.UserProfile candidate);
}
