using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly INidVerificationService _nidService;
    private readonly IPaymentService _paymentService;
    private readonly ISuccessStoryService _storyService;

    public AdminController(
        IAdminService adminService,
        INidVerificationService nidService,
        IPaymentService paymentService,
        ISuccessStoryService storyService)
    {
        _adminService = adminService;
        _nidService = nidService;
        _paymentService = paymentService;
        _storyService = storyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return View(stats);
    }

    [HttpGet]
    public async Task<IActionResult> Users(int page = 1, string? search = null, UserAccountStatus? status = null)
    {
        var result = await _adminService.GetUsersAsync(page, 15, search, status);
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> UserDetail(int id)
    {
        var user = await _adminService.GetUserDetailAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserStatus(AdminUpdateUserStatusDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var result = await _adminService.UpdateUserStatusAsync(CurrentUserId!.Value, dto, ip, ua);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction("UserDetail", new { id = dto.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> Verifications()
    {
        var pending = await _nidService.GetPendingVerificationsAsync();
        return View(pending);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewVerification(NidReviewDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _nidService.ReviewVerificationAsync(CurrentUserId!.Value, dto, ip);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction("Verifications");
    }

    [HttpGet]
    public async Task<IActionResult> Payments(int page = 1, PaymentStatus? status = null)
    {
        var result = await _paymentService.GetAllPaymentsAsync(page, 20, status);
        ViewBag.Status = status;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Logs(int page = 1)
    {
        var logs = await _adminService.GetLogsAsync(page, 25);
        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> Stories()
    {
        var stories = await _storyService.GetAllStoriesForAdminAsync();
        return View(stories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveStory(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _storyService.ApproveStoryAsync(CurrentUserId!.Value, id, ip);
        TempData["SuccessMessage"] = "Success story approved.";
        return RedirectToAction("Stories");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectStory(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _storyService.RejectStoryAsync(CurrentUserId!.Value, id, ip);
        TempData["SuccessMessage"] = "Success story rejected.";
        return RedirectToAction("Stories");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStory(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _storyService.DeleteStoryAsync(CurrentUserId!.Value, id, ip);
        TempData["SuccessMessage"] = "Success story deleted.";
        return RedirectToAction("Stories");
    }
}
