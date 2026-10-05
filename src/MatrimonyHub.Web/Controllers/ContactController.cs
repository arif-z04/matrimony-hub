using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Web.Controllers;

[Authorize]
public class ContactController : BaseController
{
    private readonly IContactAccessService _contactAccessService;
    private readonly IProfileService _profileService;

    public ContactController(IContactAccessService contactAccessService, IProfileService profileService)
    {
        _contactAccessService = contactAccessService;
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> Unlocked()
    {
        var unlockedProfiles = await _contactAccessService.GetUnlockedProfilesAsync(CurrentUserId!.Value);
        return View(unlockedProfiles);
    }

    [HttpGet]
    public async Task<IActionResult> Unlock(int profileId)
    {
        var myProfile = await _profileService.GetProfileByUserIdAsync(CurrentUserId!.Value, CurrentUserId.Value);
        if (!myProfile.Succeeded || myProfile.Data == null || !myProfile.Data.IsVerified)
        {
            TempData["ErrorMessage"] = "Identity Verification Required: Your profile must be verified via National ID (NID) before you can unlock contact information.";
            return RedirectToAction("Index", "Verification");
        }

        // Check if already unlocked
        var hasAccess = await _contactAccessService.HasAccessAsync(CurrentUserId!.Value, profileId);
        if (hasAccess)
        {
            TempData["InfoMessage"] = "You already have access to this partner's contact details.";
            return RedirectToAction("Details", "Profile", new { id = profileId });
        }

        return RedirectToAction("Checkout", "Payment", new { targetProfileId = profileId });
    }
}
