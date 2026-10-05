using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface ISuccessStoryService
{
    Task<List<SuccessStoryDto>> GetApprovedStoriesAsync();
    Task<SuccessStoryDto?> GetStoryByIdAsync(int id);
    Task<ServiceResult<int>> SubmitStoryAsync(int? userId, CreateSuccessStoryDto dto);
    Task<ServiceResult> UpdateStoryAsync(int adminUserId, UpdateSuccessStoryDto dto, string? ipAddress = null);
    Task<ServiceResult> ApproveStoryAsync(int adminUserId, int storyId, string? ipAddress = null);
    Task<ServiceResult> RejectStoryAsync(int adminUserId, int storyId, string? ipAddress = null);
    Task<ServiceResult> DeleteStoryAsync(int adminUserId, int storyId, string? ipAddress = null);
    Task<List<SuccessStoryDto>> GetAllStoriesForAdminAsync();
}
