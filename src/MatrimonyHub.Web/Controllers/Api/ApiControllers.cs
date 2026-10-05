using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int? CurrentUserId
    {
        get
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }
}

[Route("api/auth")]
public class AuthApiController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RegisterAsync(dto, ip);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(dto, ip);
        if (!result.Succeeded) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(ServiceResult.Success("Logged out"));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var result = await _authService.GeneratePasswordResetTokenAsync(dto.Email);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }
}

[Route("api/matches")]
public class MatchesApiController : BaseApiController
{
    private readonly IMatchService _matchService;

    public MatchesApiController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] MatchFilterDto filter)
    {
        var result = await _matchService.SearchMatchesAsync(filter, CurrentUserId);
        return Ok(result);
    }
}

[Route("api/favorites")]
[Authorize]
public class FavoritesApiController : BaseApiController
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesApiController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost("{profileId:int}")]
    public async Task<IActionResult> Toggle(int profileId)
    {
        var result = await _favoriteService.ToggleFavoriteAsync(CurrentUserId!.Value, profileId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var list = await _favoriteService.GetUserFavoritesAsync(CurrentUserId!.Value);
        return Ok(list);
    }
}

[Route("api/payments")]
public class PaymentsApiController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsApiController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] InitiatePaymentDto dto)
    {
        var returnUrl = Url.Action("ProcessCallback", "Payment", null, Request.Scheme)!;
        var cancelUrl = Url.Action("Failed", "Payment", null, Request.Scheme)!;

        var result = await _paymentService.InitiateContactUnlockPaymentAsync(
            CurrentUserId!.Value, dto.TargetProfileId, dto.Gateway, returnUrl, cancelUrl);

        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] PaymentCallbackDto dto)
    {
        var result = await _paymentService.ProcessPaymentCallbackAsync(dto);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> History()
    {
        var list = await _paymentService.GetUserPaymentHistoryAsync(CurrentUserId!.Value);
        return Ok(list);
    }
}

[Route("api/notifications")]
[Authorize]
public class NotificationsApiController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationsApiController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int take = 20)
    {
        var list = await _notificationService.GetUserNotificationsAsync(CurrentUserId!.Value, take);
        return Ok(list);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(CurrentUserId!.Value, id);
        return Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync(CurrentUserId!.Value);
        return Ok(result);
    }
}
