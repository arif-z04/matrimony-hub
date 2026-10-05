using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Web.Controllers;

public class SuccessStoriesController : BaseController
{
    private readonly ISuccessStoryService _storyService;

    public SuccessStoriesController(ISuccessStoryService storyService)
    {
        _storyService = storyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stories = await _storyService.GetApprovedStoriesAsync();
        return View(stories);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var story = await _storyService.GetStoryByIdAsync(id);
        if (story == null) return NotFound();
        return View(story);
    }

    [HttpGet]
    public IActionResult Submit()
    {
        return View(new CreateSuccessStoryDto { MarriageDate = DateTime.UtcNow.AddMonths(-1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(CreateSuccessStoryDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _storyService.SubmitStoryAsync(CurrentUserId, model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Thank you! Your wedding success story has been submitted.";
        return RedirectToAction("Index");
    }
}
