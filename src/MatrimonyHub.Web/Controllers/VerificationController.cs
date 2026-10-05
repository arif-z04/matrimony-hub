using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Web.Controllers;

[Authorize]
public class VerificationController : BaseController
{
    private readonly INidVerificationService _nidService;

    public VerificationController(INidVerificationService nidService)
    {
        _nidService = nidService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var status = await _nidService.GetUserVerificationStatusAsync(CurrentUserId!.Value);
        ViewBag.Status = status;
        return View(new NidSubmitDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(NidSubmitDto model)
    {
        if (!ModelState.IsValid)
        {
            var status = await _nidService.GetUserVerificationStatusAsync(CurrentUserId!.Value);
            ViewBag.Status = status;
            return View("Index", model);
        }

        var result = await _nidService.SubmitNidVerificationAsync(CurrentUserId!.Value, model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var status = await _nidService.GetUserVerificationStatusAsync(CurrentUserId!.Value);
            ViewBag.Status = status;
            return View("Index", model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Index");
    }
}
