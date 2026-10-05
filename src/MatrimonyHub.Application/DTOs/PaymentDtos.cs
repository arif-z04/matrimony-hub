using System.ComponentModel.DataAnnotations;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.DTOs;

public class InitiatePaymentDto
{
    [Required]
    public int TargetProfileId { get; set; }

    [Required]
    public PaymentGateway Gateway { get; set; } = PaymentGateway.Sandbox;
}

public class PaymentResultDto
{
    public int PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public PaymentGateway Gateway { get; set; }
    public PaymentStatus Status { get; set; }
    public string? CheckoutUrl { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

public class PaymentCallbackDto
{
    [Required]
    public string TransactionId { get; set; } = string.Empty;

    public string? GatewayTransactionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Successful;
    public string? FailureReason { get; set; }
    public string? RawPayload { get; set; }
}

public class PaymentHistoryDto
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public int? TargetProfileId { get; set; }
    public string? TargetProfileName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public PaymentPurpose Purpose { get; set; }
    public PaymentGateway Gateway { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
