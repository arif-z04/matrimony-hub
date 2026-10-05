namespace MatrimonyHub.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    Task SendWelcomeEmailAsync(string toEmail, string fullName);
    Task SendVerificationStatusEmailAsync(string toEmail, string fullName, bool approved, string? reason);
    Task SendPaymentSuccessEmailAsync(string toEmail, string fullName, string txnId, decimal amount);
    Task SendContactUnlockedEmailAsync(string toEmail, string fullName, string partnerName);
}
