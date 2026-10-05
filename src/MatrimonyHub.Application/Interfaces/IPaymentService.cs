using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Application.Interfaces;

public interface IPaymentGateway
{
    PaymentGateway GatewayType { get; }
    Task<PaymentResultDto> InitiatePaymentAsync(Payment payment, string returnUrl, string cancelUrl);
    Task<PaymentCallbackDto> VerifyCallbackAsync(Payment payment, IDictionary<string, string> callbackData);
}

public interface IPaymentService
{
    Task<ServiceResult<PaymentResultDto>> InitiateContactUnlockPaymentAsync(int userId, int targetProfileId, PaymentGateway gateway, string returnUrl, string cancelUrl);
    Task<ServiceResult<bool>> ProcessPaymentCallbackAsync(PaymentCallbackDto callbackDto);
    Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(int userId);
    Task<PagedResult<PaymentHistoryDto>> GetAllPaymentsAsync(int page, int pageSize, PaymentStatus? status = null);
}
