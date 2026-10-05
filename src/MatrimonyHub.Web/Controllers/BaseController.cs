using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MatrimonyHub.Web.Controllers;

public abstract class BaseController : Controller
{
    protected int? CurrentUserId
    {
        get
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    protected bool IsAdmin => User.IsInRole("Admin");
}
