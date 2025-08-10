using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seek.Core.Dtos.Settings.Emails;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using Seek.EF.Email_templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;

namespace Seek.EF.Repositories.System
{
    public class Repo_Email_Templates : IRepo_Email_Templates
    {
        private readonly ILogger<Repo_Email_Templates> _logger;
        private readonly EmailSettings_dto _settings;
        public Repo_Email_Templates(ILogger<Repo_Email_Templates> logger, IOptions<EmailSettings_dto> options)
        {
            _logger = logger;
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options), "Email settings cannot be null");
        }

        public async Task<bool> SendEmailAsync(string recipient, string subject, string body)
        {
            if (_settings == null)
            {
                _logger?.LogError("Email settings not configured.");
                throw new InvalidOperationException("Email settings not configured.");
            }

            if (string.IsNullOrWhiteSpace(recipient))
            {
                _logger?.LogWarning("Recipient email is null or empty.");
                throw new ArgumentException("Recipient email must be provided.", nameof(recipient));
            }

            try
            {
                using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = _settings.UseSsl
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(recipient);

                await client.SendMailAsync(message).ConfigureAwait(false);
                _logger?.LogInformation("Email sent successfully to {Recipient}", recipient);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger?.LogError(smtpEx, "SMTP error while sending email to {Recipient}", recipient);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while sending email to {Recipient}", recipient);
                throw;
            }
        }

        public async Task<bool> SendNewDeviceRegistrationAsync(string deviceId, string ipAddress, string encryptionKey, string? password = null)
        {
            if (_settings == null)
            {
                _logger?.LogWarning("Email settings not configured. Cannot send new device registration notification.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(encryptionKey))
            {
                _logger?.LogWarning("Missing required device registration data.");
                return false;
            }

            try
            {
                string subject = "Security Alert: New Device Registration";
                string body = Email_Template.NewDeviceRegistrationEmailBody(deviceId, ipAddress, encryptionKey, !string.IsNullOrEmpty(password));

                await SendEmailAsync(_settings.AdminEmail, subject, body);
                _logger?.LogInformation("New device registration notification email sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send new device registration email.");
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string recipient, string verificationCode)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                _logger?.LogError("Recipient email must be provided.");
                throw new ArgumentException("Recipient email must be provided.", nameof(recipient));
            }
            if (string.IsNullOrWhiteSpace(verificationCode))
            {
                _logger?.LogError("Verification code must be provided.");
                throw new ArgumentException("Verification code must be provided.", nameof(verificationCode));
            }
            string subject = "Seek Application Verification Code";
            string body = Email_Template.VerificationCodeEmailBody(verificationCode);

            try
            {
                await SendEmailAsync(recipient, subject, body);
                _logger?.LogInformation("Verification code email sent to {Recipient}", recipient);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send verification code email to {Recipient}", recipient);
                return false;
            }
        }

    }
}
