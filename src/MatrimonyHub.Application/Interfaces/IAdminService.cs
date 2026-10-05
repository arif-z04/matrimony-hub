using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;

namespace MatrimonyHub.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
    Task<PagedResult<AdminUserListDto>> GetUsersAsync(int page, int pageSize, string? search = null, Domain.Enums.UserAccountStatus? status = null);
    Task<AdminUserDetailDto?> GetUserDetailAsync(int userId);
    Task<ServiceResult> UpdateUserStatusAsync(int adminUserId, AdminUpdateUserStatusDto dto, string? ipAddress = null, string? userAgent = null);
    Task<PagedResult<AdminLogDto>> GetLogsAsync(int page, int pageSize);
    Task LogActionAsync(int? adminUserId, string action, string entityType, string? entityId, string description, string? ipAddress = null, string? userAgent = null);
}
