using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Infrastructure.PaymentGateways;

public class NagadPaymentGateway : IPaymentGateway
{
    private readonly PaymentGatewayOptions _options;
    private readonly ILogger<NagadPaymentGateway> _logger;

    public NagadPaymentGateway(IOptions<PaymentGatewayOptions> options, ILogger<NagadPaymentGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public PaymentGateway GatewayType => PaymentGateway.Nagad;

    public Task<PaymentResultDto> InitiatePaymentAsync(Payment payment, string returnUrl, string cancelUrl)
    {
        _logger.LogInformation("Initiating Nagad payment for TxnId: {TxnId}, Amount: {Amount}", payment.TransactionId, payment.Amount);

        var checkoutUrl = $"/checkout/gateway?gateway=Nagad&txn={payment.TransactionId}&amount={payment.Amount}";

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
            Message = "Nagad payment session initialized."
        });
    }

    public Task<PaymentCallbackDto> VerifyCallbackAsync(Payment payment, IDictionary<string, string> callbackData)
    {
        _logger.LogInformation("Verifying Nagad callback for TxnId: {TxnId}", payment.TransactionId);

        var status = callbackData.TryGetValue("status", out var s) && s.Equals("success", StringComparison.OrdinalIgnoreCase)
            ? PaymentStatus.Successful
            : PaymentStatus.Failed;

        var issuerTxn = callbackData.TryGetValue("issuer_txn", out var itxn) ? itxn : Guid.NewGuid().ToString("N")[..12].ToUpper();

        return Task.FromResult(new PaymentCallbackDto
        {
            TransactionId = payment.TransactionId,
            GatewayTransactionId = issuerTxn,
            Status = status,
            FailureReason = status == PaymentStatus.Failed ? "Nagad verification returned failed status" : null,
            RawPayload = string.Join("&", callbackData.Select(kv => $"{kv.Key}={kv.Value}"))
        });
    }
}
