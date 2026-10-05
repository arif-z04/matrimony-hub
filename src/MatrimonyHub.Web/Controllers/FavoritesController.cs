using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Web.Controllers;

[Authorize]
public class FavoritesController : BaseController
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var favorites = await _favoriteService.GetUserFavoritesAsync(CurrentUserId!.Value);
        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int profileId, string? returnUrl = null)
    {
        var result = await _favoriteService.ToggleFavoriteAsync(CurrentUserId!.Value, profileId);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }
}
