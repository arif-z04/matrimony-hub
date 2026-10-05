using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Web.Controllers;

public class PaymentController : BaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IProfileService _profileService;
    private readonly IContactAccessService _contactAccessService;
    private readonly ApplicationDbContext _db;

    public PaymentController(
        IPaymentService paymentService,
        IProfileService profileService,
        IContactAccessService contactAccessService,
        ApplicationDbContext db)
    {
        _paymentService = paymentService;
        _profileService = profileService;
        _contactAccessService = contactAccessService;
        _db = db;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Checkout(int targetProfileId)
    {
        var profileRes = await _profileService.GetProfileByIdAsync(targetProfileId, CurrentUserId);
        if (!profileRes.Succeeded || profileRes.Data == null) return NotFound();

        // Check if already unlocked
        var hasAccess = await _contactAccessService.HasAccessAsync(CurrentUserId!.Value, targetProfileId);
        if (hasAccess)
        {
            TempData["InfoMessage"] = "Contact details are already unlocked.";
            return RedirectToAction("Details", "Profile", new { id = targetProfileId });
        }

        ViewBag.TargetProfile = profileRes.Data;
        ViewBag.Fee = await _contactAccessService.GetContactUnlockFeeAsync();

        return View(new InitiatePaymentDto
        {
            TargetProfileId = targetProfileId,
            Gateway = PaymentGateway.Sandbox
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Initiate(InitiatePaymentDto model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Checkout", new { targetProfileId = model.TargetProfileId });
        }

        var returnUrl = Url.Action("Callback", "Payment", null, Request.Scheme)!;
        var cancelUrl = Url.Action("Failed", "Payment", null, Request.Scheme)!;

        var result = await _paymentService.InitiateContactUnlockPaymentAsync(
            CurrentUserId!.Value, model.TargetProfileId, model.Gateway, returnUrl, cancelUrl);

        if (!result.Succeeded || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Checkout", new { targetProfileId = model.TargetProfileId });
        }

        return Redirect(result.Data.CheckoutUrl ?? "/Payment/History");
    }

    // Realistic Gateway authorization portal (Simulates bKash / SSLCommerz / Nagad / Sandbox checkout)
    [HttpGet]
    [Route("checkout/gateway")]
    public async Task<IActionResult> GatewayCheckout(string gateway, string txn, decimal amount)
    {
        var payment = await _db.Payments
            .Include(p => p.TargetProfile)
            .FirstOrDefaultAsync(p => p.TransactionId == txn);

        if (payment == null) return NotFound("Payment transaction not found.");

        ViewBag.Gateway = gateway;
        ViewBag.Txn = txn;
        ViewBag.Amount = amount;
        ViewBag.TargetProfile = payment.TargetProfile;

        return View("GatewayCheckout");
    }

    // Server-side Callback verification endpoint
    [HttpPost]
    [Route("api/payments/callback")]
    [Route("Payment/ProcessCallback")]
    public async Task<IActionResult> ProcessCallback([FromForm] PaymentCallbackDto dto)
    {
        var result = await _paymentService.ProcessPaymentCallbackAsync(dto);
        if (result.Succeeded && result.Data)
        {
            return RedirectToAction("Success", new { txn = dto.TransactionId });
        }

        return RedirectToAction("Failed", new { txn = dto.TransactionId, reason = result.Message });
    }

    [HttpGet]
    public async Task<IActionResult> Success(string txn)
    {
        var payment = await _db.Payments
            .Include(p => p.TargetProfile)
            .FirstOrDefaultAsync(p => p.TransactionId == txn);

        if (payment == null) return NotFound();

        ViewBag.Payment = payment;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Failed(string txn, string? reason = null)
    {
        var payment = await _db.Payments
            .Include(p => p.TargetProfile)
            .FirstOrDefaultAsync(p => p.TransactionId == txn);

        ViewBag.Payment = payment;
        ViewBag.Reason = reason ?? payment?.FailureReason ?? "The payment transaction could not be completed.";
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var history = await _paymentService.GetUserPaymentHistoryAsync(CurrentUserId!.Value);
        return View(history);
    }
}
