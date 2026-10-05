using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Domain.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public virtual Payment Payment { get; set; } = null!;

    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
