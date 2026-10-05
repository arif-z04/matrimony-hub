using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface IContactAccessService
{
    Task<bool> HasAccessAsync(int userId, int targetProfileId);
    Task<ServiceResult> UnlockContactAsync(int userId, int targetProfileId, int paymentId);
    Task<List<ProfileCardDto>> GetUnlockedProfilesAsync(int userId);
    Task<decimal> GetContactUnlockFeeAsync();
}
