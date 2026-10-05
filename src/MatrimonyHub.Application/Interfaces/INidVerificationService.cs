using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface INidVerificationService
{
    Task<ServiceResult> SubmitNidVerificationAsync(int userId, NidSubmitDto dto);
    Task<NidVerificationDetailDto?> GetUserVerificationStatusAsync(int userId);
    Task<List<NidVerificationDetailDto>> GetPendingVerificationsAsync();
    Task<ServiceResult> ReviewVerificationAsync(int adminUserId, NidReviewDto dto, string? ipAddress = null);
}
