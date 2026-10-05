using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ApplicationDbContext _db;
    private readonly ISuccessStoryService _storyService;
    private readonly IMatchService _matchService;

    public HomeController(
        ApplicationDbContext db,
        ISuccessStoryService storyService,
        IMatchService matchService)
    {
        _db = db;
        _storyService = storyService;
        _matchService = matchService;
    }

    public async Task<IActionResult> Index()
    {
        // Real database statistics - strictly NO fake numbers!
        var verifiedMembersCount = await _db.UserProfiles.CountAsync(p => p.IsVerified && p.IsActive);
        var totalProfilesCount = await _db.UserProfiles.CountAsync(p => p.IsActive);
        var successStoriesCount = await _db.SuccessStories.CountAsync(s => s.Status == Domain.Enums.SuccessStoryStatus.Approved);
        var successfulMatchesCount = await _db.ContactAccesses.CountAsync();

        ViewBag.VerifiedCount = verifiedMembersCount;
        ViewBag.TotalProfilesCount = totalProfilesCount;
        ViewBag.SuccessStoriesCount = successStoriesCount;
        ViewBag.SuccessfulMatchesCount = successfulMatchesCount;

        // Featured Success Stories
        var featuredStories = await _storyService.GetApprovedStoriesAsync();
        ViewBag.FeaturedStories = featuredStories.Take(3).ToList();

        // Sample Verified Profiles
        var verifiedProfilesResult = await _matchService.SearchMatchesAsync(new MatchFilterDto
        {
            VerifiedOnly = true,
            PageSize = 4,
            Page = 1
        }, CurrentUserId);

        ViewBag.FeaturedProfiles = verifiedProfilesResult.Items;

        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Route("Home/Error/{statusCode:int?}")]
    public IActionResult Error(int? statusCode = null)
    {
        var code = statusCode ?? HttpContext.Response.StatusCode;
        ViewBag.StatusCode = code;

        return code switch
        {
            404 => View("Error404"),
            403 => View("Error403"),
            _ => View("Error500")
        };
    }
}
