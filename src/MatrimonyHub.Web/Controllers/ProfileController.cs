using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Web.Controllers;

public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;
    private readonly IContactAccessService _contactAccessService;

    public ProfileController(IProfileService profileService, IContactAccessService contactAccessService)
    {
        _profileService = profileService;
        _contactAccessService = contactAccessService;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _profileService.GetProfileByIdAsync(id, CurrentUserId);
        if (!result.Succeeded || result.Data == null)
        {
            return NotFound();
        }

        ViewBag.UnlockFee = await _contactAccessService.GetContactUnlockFeeAsync();

        bool isCurrentUserVerified = false;
        if (CurrentUserId.HasValue)
        {
            var myProfile = await _profileService.GetProfileByUserIdAsync(CurrentUserId.Value, CurrentUserId.Value);
            if (myProfile.Succeeded && myProfile.Data != null)
            {
                isCurrentUserVerified = myProfile.Data.IsVerified;
            }
        }
        ViewBag.IsCurrentUserVerified = isCurrentUserVerified;

        return View(result.Data);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var result = await _profileService.GetProfileByUserIdAsync(CurrentUserId!.Value, CurrentUserId.Value);
        if (!result.Succeeded || result.Data == null) return NotFound();

        var p = result.Data;
        var model = new UpdateProfileDto
        {
            FullName = p.FullName,
            Gender = p.Gender,
            DateOfBirth = p.DateOfBirth,
            HeightCm = p.HeightCm,
            WeightKg = p.WeightKg,
            MaritalStatus = p.MaritalStatus,
            Religion = p.Religion,
            MotherTongue = p.MotherTongue,
            Nationality = p.Nationality,
            HighestEducation = p.HighestEducation,
            Institution = p.Institution,
            Subject = p.Subject,
            GraduationYear = p.GraduationYear,
            Occupation = p.Occupation,
            Company = p.Company,
            JobTitle = p.JobTitle,
            IncomeRange = p.IncomeRange,
            Division = p.Division,
            District = p.District,
            City = p.City,
            Country = p.Country,
            Smoking = p.Smoking,
            Drinking = p.Drinking,
            DietaryPreference = p.DietaryPreference,
            Hobbies = p.Hobbies,
            Interests = p.Interests,
            AboutMe = p.AboutMe,
            FamilyInfo = p.FamilyInfo,
            PartnerExpectations = p.PartnerExpectations
        };

        ViewBag.Profile = p;
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProfileDto model)
    {
        if (!ModelState.IsValid)
        {
            var pRes = await _profileService.GetProfileByUserIdAsync(CurrentUserId!.Value, CurrentUserId.Value);
            ViewBag.Profile = pRes.Data;
            return View(model);
        }

        var result = await _profileService.UpdateProfileAsync(CurrentUserId!.Value, model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var pRes = await _profileService.GetProfileByUserIdAsync(CurrentUserId.Value, CurrentUserId.Value);
            ViewBag.Profile = pRes.Data;
            return View(model);
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction("Edit");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Preferences()
    {
        var result = await _profileService.GetProfileByUserIdAsync(CurrentUserId!.Value, CurrentUserId.Value);
        if (!result.Succeeded || result.Data == null) return NotFound();

        var pref = result.Data.PartnerPreference;
        var model = new UpdatePreferencesDto
        {
            MinAge = pref?.MinAge,
            MaxAge = pref?.MaxAge,
            PreferredGender = pref?.PreferredGender,
            PreferredReligion = pref?.PreferredReligion,
            PreferredMaritalStatus = pref?.PreferredMaritalStatus,
            PreferredEducation = pref?.PreferredEducation,
            PreferredOccupation = pref?.PreferredOccupation,
            PreferredDivision = pref?.PreferredDivision,
            DietaryPreference = pref?.DietaryPreference,
            Notes = pref?.Notes
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preferences(UpdatePreferencesDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _profileService.UpdatePreferencesAsync(CurrentUserId!.Value, model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Partner preferences updated successfully.";
        return RedirectToAction("Preferences");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(UploadPhotoDto dto)
    {
        var result = await _profileService.UploadPhotoAsync(CurrentUserId!.Value, dto);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Photo uploaded successfully.";
        }

        return RedirectToAction("Edit");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryPhoto(int photoId)
    {
        var result = await _profileService.SetPrimaryPhotoAsync(CurrentUserId!.Value, photoId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Primary photo updated.";
        }
        return RedirectToAction("Edit");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        var result = await _profileService.DeletePhotoAsync(CurrentUserId!.Value, photoId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Photo deleted.";
        }
        return RedirectToAction("Edit");
    }
}
