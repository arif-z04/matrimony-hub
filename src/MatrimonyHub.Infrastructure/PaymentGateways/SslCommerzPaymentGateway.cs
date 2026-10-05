using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Infrastructure.PaymentGateways;

public class SslCommerzPaymentGateway : IPaymentGateway
{
    private readonly PaymentGatewayOptions _options;
    private readonly ILogger<SslCommerzPaymentGateway> _logger;

    public SslCommerzPaymentGateway(IOptions<PaymentGatewayOptions> options, ILogger<SslCommerzPaymentGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public PaymentGateway GatewayType => PaymentGateway.SSLCommerz;

    public Task<PaymentResultDto> InitiatePaymentAsync(Payment payment, string returnUrl, string cancelUrl)
    {
        _logger.LogInformation("Initiating SSLCommerz payment for TxnId: {TxnId}, Amount: {Amount}", payment.TransactionId, payment.Amount);

        var checkoutUrl = $"/checkout/gateway?gateway=SSLCommerz&txn={payment.TransactionId}&amount={payment.Amount}";

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
            Message = "SSLCommerz session created."
        });
    }

    public Task<PaymentCallbackDto> VerifyCallbackAsync(Payment payment, IDictionary<string, string> callbackData)
    {
        _logger.LogInformation("Verifying SSLCommerz callback for TxnId: {TxnId}", payment.TransactionId);

        var status = callbackData.TryGetValue("status", out var s) && (s.Equals("VALID", StringComparison.OrdinalIgnoreCase) || s.Equals("success", StringComparison.OrdinalIgnoreCase))
            ? PaymentStatus.Successful
            : PaymentStatus.Failed;

        var valId = callbackData.TryGetValue("val_id", out var v) ? v : Guid.NewGuid().ToString("N")[..12].ToUpper();

        return Task.FromResult(new PaymentCallbackDto
        {
            TransactionId = payment.TransactionId,
            GatewayTransactionId = valId,
            Status = status,
            FailureReason = status == PaymentStatus.Failed ? "SSLCommerz payment validation failed" : null,
            RawPayload = string.Join("&", callbackData.Select(kv => $"{kv.Key}={kv.Value}"))
        });
    }
}
