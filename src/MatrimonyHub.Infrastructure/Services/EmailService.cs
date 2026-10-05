using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPortStr = _config["Email:SmtpPort"];
        var smtpUser = _config["Email:Username"];
        var smtpPass = _config["Email:Password"];
        var fromEmail = _config["Email:FromEmail"] ?? "noreply@matrimonyhub.com";
        var fromName = _config["Email:FromName"] ?? "Matrimony Hub";

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser))
        {
            _logger.LogInformation("[Email Mock/Dev] To: {To} | Subject: {Subject} | Body Length: {Len}", toEmail, subject, htmlMessage.Length);
            return;
        }

        try
        {
            var port = int.TryParse(smtpPortStr, out var p) ? p : 587;
            using var client = new SmtpClient(smtpHost, port)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
            _logger.LogInformation("Email sent successfully to {To}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", toEmail);
        }
    }

    public Task SendWelcomeEmailAsync(string toEmail, string fullName) =>
        SendEmailAsync(toEmail, "Welcome to Matrimony Hub",
            $"<h3>Welcome to Matrimony Hub, {fullName}!</h3><p>We are delighted to have you join our verified matrimonial community. Please complete your profile and verify your NID to get the best match suggestions.</p>");

    public Task SendVerificationStatusEmailAsync(string toEmail, string fullName, bool approved, string? reason) =>
        SendEmailAsync(toEmail, approved ? "NID Verification Approved - Matrimony Hub" : "NID Verification Status Update",
            approved
                ? $"<p>Dear {fullName},</p><p>Congratulations! Your National ID (NID) has been successfully verified. A verified badge is now displayed on your matrimonial profile.</p>"
                : $"<p>Dear {fullName},</p><p>Your recent NID verification could not be approved for the following reason: <strong>{reason}</strong>. Please review your documents and submit again.</p>");

    public Task SendPaymentSuccessEmailAsync(string toEmail, string fullName, string txnId, decimal amount) =>
        SendEmailAsync(toEmail, "Payment Confirmation - Matrimony Hub",
            $"<p>Dear {fullName},</p><p>Thank you for your payment. Your transaction <strong>{txnId}</strong> for <strong>BDT {amount:N2}</strong> has been successfully processed. The requested profile contact details have been unlocked.</p>");

    public Task SendContactUnlockedEmailAsync(string toEmail, string fullName, string partnerName) =>
        SendEmailAsync(toEmail, "Contact Information Unlocked",
            $"<p>Dear {fullName},</p><p>You now have verified access to direct contact information for <strong>{partnerName}</strong>. You can view these details on their profile or in your dashboard.</p>");
}
