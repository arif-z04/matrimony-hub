using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface IFavoriteService
{
    Task<ServiceResult> ToggleFavoriteAsync(int userId, int targetProfileId);
    Task<List<ProfileCardDto>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int targetProfileId);
}
