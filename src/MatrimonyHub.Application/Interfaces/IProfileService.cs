using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface IProfileService
{
    Task<ServiceResult<ProfileDetailDto>> GetProfileByUserIdAsync(int targetUserId, int? requestingUserId = null);
    Task<ServiceResult<ProfileDetailDto>> GetProfileByIdAsync(int profileId, int? requestingUserId = null);
    Task<ServiceResult> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<ServiceResult> UpdatePreferencesAsync(int userId, UpdatePreferencesDto dto);
    Task<ServiceResult<string>> UploadPhotoAsync(int userId, UploadPhotoDto dto);
    Task<ServiceResult> SetPrimaryPhotoAsync(int userId, int photoId);
    Task<ServiceResult> DeletePhotoAsync(int userId, int photoId);
    Task<int> CalculateProfileCompletionAsync(int userId);
}
