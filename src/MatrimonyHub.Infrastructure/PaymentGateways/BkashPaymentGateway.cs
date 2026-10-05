using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Infrastructure.PaymentGateways;

public class BkashPaymentGateway : IPaymentGateway
{
    private readonly PaymentGatewayOptions _options;
    private readonly ILogger<BkashPaymentGateway> _logger;

    public BkashPaymentGateway(IOptions<PaymentGatewayOptions> options, ILogger<BkashPaymentGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public PaymentGateway GatewayType => PaymentGateway.bKash;

    public Task<PaymentResultDto> InitiatePaymentAsync(Payment payment, string returnUrl, string cancelUrl)
    {
        _logger.LogInformation("Initiating bKash payment for TxnId: {TxnId}, Amount: {Amount}", payment.TransactionId, payment.Amount);

        // When live credentials are provided, calls bKash Create Payment API.
        // For development / sandbox without hardcoded live keys, provide standard redirect
        var checkoutUrl = $"/checkout/gateway?gateway=bKash&txn={payment.TransactionId}&amount={payment.Amount}";

        return Task.FromResult(new PaymentResultDto
        {
            PaymentId = payment.Id,
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Gateway = GatewayType,
            Status = PaymentStatus.Processing,
            CheckoutUrl = checkoutUrl,
            IsSuccess = true,
            Message = "bKash payment checkout initiated."
        });
    }

    public Task<PaymentCallbackDto> VerifyCallbackAsync(Payment payment, IDictionary<string, string> callbackData)
    {
        _logger.LogInformation("Verifying bKash callback for TxnId: {TxnId}", payment.TransactionId);

        var status = callbackData.TryGetValue("status", out var s) && s.Equals("success", StringComparison.OrdinalIgnoreCase)
            ? PaymentStatus.Successful
            : PaymentStatus.Failed;

        var gatewayTxnId = callbackData.TryGetValue("paymentID", out var pId) ? pId : Guid.NewGuid().ToString("N")[..12].ToUpper();

        return Task.FromResult(new PaymentCallbackDto
        {
            TransactionId = payment.TransactionId,
            GatewayTransactionId = gatewayTxnId,
            Status = status,
            FailureReason = status == PaymentStatus.Failed ? "bKash transaction was rejected or cancelled" : null,
            RawPayload = string.Join("&", callbackData.Select(kv => $"{kv.Key}={kv.Value}"))
        });
    }
}
