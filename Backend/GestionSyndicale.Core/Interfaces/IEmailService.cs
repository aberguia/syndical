namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service d'envoi d'emails via SMTP
/// </summary>
public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string firstName);
    Task<bool> SendPaymentReceiptEmailAsync(string toEmail, string firstName, decimal amount, string receiptPath);
    Task<bool> SendNewsNotificationEmailAsync(string toEmail, string firstName, string newsTitle);
    Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body);
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string firstName, string temporaryPassword);
}
