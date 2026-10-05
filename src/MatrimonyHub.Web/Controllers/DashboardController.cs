using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Web.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _db;
    private readonly IProfileService _profileService;
    private readonly IMatchService _matchService;
    private readonly INotificationService _notificationService;
    private readonly INidVerificationService _nidService;
    private readonly IPaymentService _paymentService;

    public DashboardController(
        ApplicationDbContext db,
        IProfileService profileService,
        IMatchService matchService,
        INotificationService notificationService,
        INidVerificationService nidService,
        IPaymentService paymentService)
    {
        _db = db;
        _profileService = profileService;
        _matchService = matchService;
        _notificationService = notificationService;
        _nidService = nidService;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId!.Value;

        // User profile & completion
        var profileRes = await _profileService.GetProfileByUserIdAsync(userId, userId);
        var completionScore = await _profileService.CalculateProfileCompletionAsync(userId);
        ViewBag.Profile = profileRes.Data;
        ViewBag.CompletionScore = completionScore;

        // Verification status
        var verification = await _nidService.GetUserVerificationStatusAsync(userId);
        ViewBag.Verification = verification;

        // Suggested Matches
        var suggestions = await _matchService.GetSuggestedMatchesAsync(userId, 4);
        ViewBag.Suggestions = suggestions;

        // Real counts from Database
        var favoritesCount = await _db.Favorites.CountAsync(f => f.UserId == userId);
        var unlockedCount = await _db.ContactAccesses.CountAsync(c => c.UserId == userId);
        var unreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

        ViewBag.FavoritesCount = favoritesCount;
        ViewBag.UnlockedCount = unlockedCount;
        ViewBag.UnreadNotificationsCount = unreadNotifications;

        // Recent Notifications
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 5);
        ViewBag.RecentNotifications = notifications;

        // Recent Payments
        var payments = await _paymentService.GetUserPaymentHistoryAsync(userId);
        ViewBag.RecentPayments = payments.Take(4).ToList();

        return View();
    }
}
