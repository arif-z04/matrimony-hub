using Microsoft.Extensions.Logging;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Infrastructure.PaymentGateways;

public class SandboxPaymentGateway : IPaymentGateway
{
    private readonly ILogger<SandboxPaymentGateway> _logger;

    public SandboxPaymentGateway(ILogger<SandboxPaymentGateway> logger)
    {
        _logger = logger;
    }

    public PaymentGateway GatewayType => PaymentGateway.Sandbox;

    public Task<PaymentResultDto> InitiatePaymentAsync(Payment payment, string returnUrl, string cancelUrl)
    {
        _logger.LogInformation("Sandbox payment initiated for TxnId: {TxnId}, Amount: {Amount}", payment.TransactionId, payment.Amount);

        var checkoutUrl = $"/checkout/gateway?gateway=Sandbox&txn={payment.TransactionId}&amount={payment.Amount}";

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
            Message = "Ready for authorization."
        });
    }

    public Task<PaymentCallbackDto> VerifyCallbackAsync(Payment payment, IDictionary<string, string> callbackData)
    {
        _logger.LogInformation("Verifying sandbox callback for TxnId: {TxnId}", payment.TransactionId);

        var status = callbackData.TryGetValue("status", out var s) && s.Equals("success", StringComparison.OrdinalIgnoreCase)
            ? PaymentStatus.Successful
            : PaymentStatus.Failed;

        var gTxn = "SBX-" + Guid.NewGuid().ToString("N")[..10].ToUpper();

        return Task.FromResult(new PaymentCallbackDto
        {
            TransactionId = payment.TransactionId,
            GatewayTransactionId = gTxn,
            Status = status,
            FailureReason = status == PaymentStatus.Failed ? "Payment was cancelled or rejected by user" : null,
            RawPayload = "SandboxAuth=Confirmed"
        });
    }
}
