using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<UserSummaryDto>> RegisterAsync(RegisterRequestDto request, string? ipAddress = null);
    Task<ServiceResult<UserSummaryDto>> LoginAsync(LoginRequestDto request, string? ipAddress = null);
    Task<ServiceResult> LogoutAsync();
    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string email);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<UserSummaryDto?> GetCurrentUserSummaryAsync(int userId);
}
