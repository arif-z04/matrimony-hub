using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.PaymentGateways;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IContactAccessService _contactAccessService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly PaymentGatewayOptions _options;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext db,
        IEnumerable<IPaymentGateway> gateways,
        IContactAccessService contactAccessService,
        INotificationService notificationService,
        IEmailService emailService,
        IOptions<PaymentGatewayOptions> options,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _gateways = gateways;
        _contactAccessService = contactAccessService;
        _notificationService = notificationService;
        _emailService = emailService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentResultDto>> InitiateContactUnlockPaymentAsync(
        int userId, int targetProfileId, PaymentGateway gateway, string returnUrl, string cancelUrl)
    {
        var targetProfile = await _db.UserProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == targetProfileId);

        if (targetProfile == null)
            return ServiceResult<PaymentResultDto>.Failure("Target matrimonial profile not found.");

        if (targetProfile.UserId == userId)
            return ServiceResult<PaymentResultDto>.Failure("You cannot purchase contact details for your own profile.");

        // Check if already unlocked
        var alreadyUnlocked = await _contactAccessService.HasAccessAsync(userId, targetProfileId);
        if (alreadyUnlocked)
        {
            return ServiceResult<PaymentResultDto>.Failure("You have already unlocked contact details for this profile.");
        }

        // Check pending payment within last 15 minutes to avoid duplicates
        var pendingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TargetProfileId == targetProfileId && p.Status == PaymentStatus.Pending && p.CreatedAt > DateTime.UtcNow.AddMinutes(-15));

        var fee = _options.ContactUnlockFee > 0 ? _options.ContactUnlockFee : 500.00m;

        Payment payment;
        if (pendingPayment != null)
        {
            payment = pendingPayment;
            payment.Gateway = gateway;
        }
        else
        {
            var txnId = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
            payment = new Payment
            {
                UserId = userId,
                TargetProfileId = targetProfileId,
                Amount = fee,
                Currency = "BDT",
                PaymentPurpose = PaymentPurpose.ContactUnlock,
                TransactionId = txnId,
                Gateway = gateway,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
        }

        var selectedGateway = _gateways.FirstOrDefault(g => g.GatewayType == gateway)
                              ?? _gateways.First(g => g.GatewayType == PaymentGateway.Sandbox);

        var gatewayResult = await selectedGateway.InitiatePaymentAsync(payment, returnUrl, cancelUrl);

        payment.Status = PaymentStatus.Processing;
        await _db.SaveChangesAsync();

        return ServiceResult<PaymentResultDto>.Success(gatewayResult, "Payment checkout initiated.");
    }

    public async Task<ServiceResult<bool>> ProcessPaymentCallbackAsync(PaymentCallbackDto callbackDto)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var payment = await _db.Payments
                    .Include(p => p.User)
                    .Include(p => p.TargetProfile)
                    .FirstOrDefaultAsync(p => p.TransactionId == callbackDto.TransactionId);

                if (payment == null)
                {
                    return ServiceResult<bool>.Failure("Payment record not found for transaction: " + callbackDto.TransactionId);
                }

                // Idempotency: if already processed as successful, skip duplicate processing
                if (payment.Status == PaymentStatus.Successful)
                {
                    await transaction.CommitAsync();
                    return ServiceResult<bool>.Success(true, "Transaction already completed successfully.");
                }

                payment.GatewayTransactionId = callbackDto.GatewayTransactionId;
                payment.CompletedAt = DateTime.UtcNow;

                // Audit record for this gateway transaction
                var pTransaction = new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    GatewayTransactionId = callbackDto.GatewayTransactionId,
                    GatewayResponse = callbackDto.RawPayload ?? callbackDto.Status.ToString(),
                    Amount = payment.Amount,
                    Status = callbackDto.Status,
                    CreatedAt = DateTime.UtcNow
                };
                _db.PaymentTransactions.Add(pTransaction);

                if (callbackDto.Status == PaymentStatus.Successful)
                {
                    payment.Status = PaymentStatus.Successful;
                    payment.FailureReason = null;
                    await _db.SaveChangesAsync();

                    // Unlock contact access
                    if (payment.TargetProfileId.HasValue)
                    {
                        var unlockResult = await _contactAccessService.UnlockContactAsync(
                            payment.UserId, payment.TargetProfileId.Value, payment.Id);

                        if (!unlockResult.Succeeded)
                        {
                            _logger.LogWarning("Contact unlock warning: {Msg}", unlockResult.Message);
                        }

                        // Send notification to buyer
                        var partnerName = payment.TargetProfile?.FullName ?? "Partner";
                        await _notificationService.CreateNotificationAsync(
                            payment.UserId,
                            "Contact Unlocked",
                            $"You have successfully unlocked contact information for {partnerName}.",
                            NotificationType.ContactUnlocked,
                            payment.TargetProfileId.Value.ToString());

                        // Send notification to target profile
                        if (payment.TargetProfile != null)
                        {
                            await _notificationService.CreateNotificationAsync(
                                payment.TargetProfile.UserId,
                                "Contact Information Requested",
                                $"{payment.User.FullName} unlocked your contact details and may reach out to you soon.",
                                NotificationType.ProfileViewed,
                                payment.UserId.ToString());
                        }

                        if (!string.IsNullOrWhiteSpace(payment.User.Email))
                        {
                            await _emailService.SendPaymentSuccessEmailAsync(
                                payment.User.Email, payment.User.FullName, payment.TransactionId, payment.Amount);
                        }
                    }
                }
                else
                {
                    payment.Status = callbackDto.Status;
                    payment.FailureReason = callbackDto.FailureReason ?? "Payment failed or was cancelled.";
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return ServiceResult<bool>.Success(payment.Status == PaymentStatus.Successful,
                    payment.Status == PaymentStatus.Successful ? "Payment verified and contact unlocked." : "Payment failed.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing payment callback for txn {TxnId}", callbackDto.TransactionId);
                return ServiceResult<bool>.Failure("Transaction processing encountered an internal error.");
            }
        });
    }

    public async Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(int userId)
    {
        var list = await _db.Payments
            .Where(p => p.UserId == userId)
            .Include(p => p.TargetProfile)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return list.Select(p => new PaymentHistoryDto
        {
            Id = p.Id,
            TransactionId = p.TransactionId,
            TargetProfileId = p.TargetProfileId,
            TargetProfileName = p.TargetProfile?.FullName,
            Amount = p.Amount,
            Currency = p.Currency,
            Purpose = p.PaymentPurpose,
            Gateway = p.Gateway,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt
        }).ToList();
    }

    public async Task<PagedResult<PaymentHistoryDto>> GetAllPaymentsAsync(int page, int pageSize, PaymentStatus? status = null)
    {
        var query = _db.Payments
            .Include(p => p.User)
            .Include(p => p.TargetProfile)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = list.Select(p => new PaymentHistoryDto
        {
            Id = p.Id,
            TransactionId = p.TransactionId,
            TargetProfileId = p.TargetProfileId,
            TargetProfileName = p.TargetProfile?.FullName ?? p.User?.FullName,
            Amount = p.Amount,
            Currency = p.Currency,
            Purpose = p.PaymentPurpose,
            Gateway = p.Gateway,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt
        }).ToList();

        return new PagedResult<PaymentHistoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
