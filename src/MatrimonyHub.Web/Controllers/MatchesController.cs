using Microsoft.AspNetCore.Mvc;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Web.Controllers;

public class MatchesController : BaseController
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] MatchFilterDto filter)
    {
        var result = await _matchService.SearchMatchesAsync(filter, CurrentUserId);
        ViewBag.Filter = filter;
        return View(result);
    }
}
