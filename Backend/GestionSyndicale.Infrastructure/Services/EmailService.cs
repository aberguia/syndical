using GestionSyndicale.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace GestionSyndicale.Infrastructure.Services;

/// <summary>
/// Service d'envoi d'emails via SMTP (serveur de la résidence)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("SMTP Host not configured");
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUser = _configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("SMTP User not configured");
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("SMTP Password not configured");
        _fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("From Email not configured");
        _fromName = _configuration["Email:FromName"] ?? "Gestion Syndicale";
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string firstName)
    {
        var subject = "Code de validation de votre compte";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Bonjour {firstName},</h2>
                <p>Votre code de validation est :</p>
                <h1 style='color: #4CAF50; font-size: 36px; letter-spacing: 5px;'>{otpCode}</h1>
                <p>Ce code est valide pendant <strong>15 minutes</strong>.</p>
                <p>Si vous n'avez pas demandé ce code, veuillez ignorer cet email.</p>
                <br/>
                <p>Cordialement,<br/>L'équipe de gestion syndicale</p>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, firstName, subject, body);
    }

    public async Task<bool> SendPaymentReceiptEmailAsync(string toEmail, string firstName, decimal amount, string receiptPath)
    {
        var subject = $"Reçu de paiement - {amount:C}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Bonjour {firstName},</h2>
                <p>Nous accusons réception de votre paiement de <strong>{amount:C}</strong>.</p>
                <p>Votre reçu est disponible en pièce jointe.</p>
                <p>Vous pouvez également le consulter à tout moment depuis votre espace adhérent.</p>
                <br/>
                <p>Merci pour votre confiance,<br/>L'équipe de gestion syndicale</p>
            </body>
            </html>
        ";

        return await SendEmailWithAttachmentAsync(toEmail, subject, body, receiptPath);
    }

    public async Task<bool> SendNewsNotificationEmailAsync(string toEmail, string firstName, string newsTitle)
    {
        var subject = $"Nouvelle publication : {newsTitle}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Bonjour {firstName},</h2>
                <p>Une nouvelle actualité a été publiée :</p>
                <h3 style='color: #2196F3;'>{newsTitle}</h3>
                <p>Connectez-vous à votre espace adhérent pour la consulter.</p>
                <br/>
                <p>Cordialement,<br/>L'équipe de gestion syndicale</p>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, firstName, subject, body);
    }

    public async Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            
            foreach (var recipient in recipients)
            {
                message.Bcc.Add(MailboxAddress.Parse(recipient)); // BCC pour confidentialité
            }

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUser, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // Log l'erreur (à implémenter avec ILogger)
            Console.WriteLine($"Erreur envoi email groupé: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUser, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur envoi email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string firstName, string temporaryPassword)
    {
        var subject = "Bienvenue - Votre compte adhérent";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Bonjour {firstName},</h2>
                <p>Votre compte adhérent a été créé avec succès.</p>
                <p>Voici vos identifiants de connexion :</p>
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <p><strong>Email :</strong> {toEmail}</p>
                    <p><strong>Mot de passe temporaire :</strong> <span style='color: #4CAF50; font-size: 18px; font-weight: bold;'>{temporaryPassword}</span></p>
                </div>
                <p style='color: #ff5722;'><strong>Important :</strong> Pour des raisons de sécurité, veuillez changer ce mot de passe lors de votre première connexion.</p>
                <p>Vous pouvez vous connecter à votre espace adhérent en utilisant ces identifiants.</p>
                <br/>
                <p>Cordialement,<br/>L'équipe de gestion syndicale</p>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, firstName, subject, body);
    }

    private async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, string? attachmentPath = null)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };

            // Ajouter une pièce jointe si fournie
            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                bodyBuilder.Attachments.Add(attachmentPath);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUser, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // Log l'erreur (à implémenter avec ILogger)
            Console.WriteLine($"Erreur envoi email: {ex.Message}");
            return false;
        }
    }
}
