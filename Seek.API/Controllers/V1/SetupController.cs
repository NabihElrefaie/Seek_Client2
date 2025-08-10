using Microsoft.AspNetCore.Mvc;
using Seek.API.Security.Config;
using Seek.Core.Dtos.Settings.Emails;
using System.IO;


namespace Seek.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiVersion("1.0")]
    public class SetupController : ControllerBase
    {
        private readonly ILogger<SetupController> _logger;

        public SetupController(ILogger<SetupController> logger)
        {
            _logger = logger;
        }

        [HttpPost("set-email-config-key")]
        public async Task<IActionResult> SetEmailConfigKey([FromBody] EmailKeySetupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                return BadRequest(new { message = "Encryption key cannot be empty" });
            }

            try
            {
                // Paths for configuration files
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "email.cfg");
                string keyPath = Path.Combine(Directory.GetCurrentDirectory(), "email.key");

                // Check if the email configuration already exists
                if (System.IO.File.Exists(configPath) && System.IO.File.Exists(keyPath))
                {
                    // Try to decrypt with the provided key first to validate
                    var settings = new EmailSettings_dto();
                    var logger = _logger;
                    bool success = EmailConfiguration.ConfigureFromSecureFilesWithKey(settings, logger, configPath, keyPath, request.Key);

                    if (!success)
                    {
                        return BadRequest(new { message = "Invalid key for existing configuration" });
                    }

                    return Ok(new { message = "Email configuration key validated successfully" });
                }

                // If files don't exist, create a new configuration with the provided key
                var newSettings = new EmailSettings_dto
                {
                    SmtpServer = request.SmtpServer ?? "smtp.example.com",
                    SmtpPort = request.SmtpPort ?? 587,
                    Username = request.Username ?? "user@example.com",
                    Password = request.Password ?? "defaultpassword",
                    FromEmail = request.FromEmail ?? "noreply@example.com",
                    AdminEmail = request.AdminEmail ?? "admin@example.com",
                    UseSsl = request.UseSsl ?? true
                };

                // Create and encrypt the configuration
                bool created = await Task.Run(() => EmailConfiguration.SaveSecureConfig(
                    newSettings, _logger, configPath, keyPath, request.Key));

                if (!created)
                {
                    return StatusCode(500, new { message = "Failed to create email configuration" });
                }

                return Ok(new
                {
                    message = "Email configuration created successfully",
                    configPath = configPath,
                    keyPath = keyPath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting email configuration key");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("verify-email-config")]
        public IActionResult VerifyEmailConfig([FromQuery] string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest(new { message = "Key cannot be empty" });
            }

            try
            {
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "email.cfg");
                string keyPath = Path.Combine(Directory.GetCurrentDirectory(), "email.key");

                if (!System.IO.File.Exists(configPath) || !System.IO.File.Exists(keyPath))
                {
                    return NotFound(new { message = "Email configuration files not found" });
                }

                var settings = new EmailSettings_dto();
                bool success = EmailConfiguration.ConfigureFromSecureFilesWithKey(
                    settings, _logger, configPath, keyPath, key);

                if (success)
                {
                    return Ok(new
                    {
                        message = "Email configuration is valid",
                        smtpServer = settings.SmtpServer,
                        smtpPort = settings.SmtpPort,
                        username = settings.Username,
                        fromEmail = settings.FromEmail,
                        adminEmail = settings.AdminEmail,
                        useSsl = settings.UseSsl
                    });
                }
                else
                {
                    return BadRequest(new { message = "Invalid configuration key" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email configuration");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }

    public class EmailKeySetupRequest
    {
        public string Key { get; set; }
        public string SmtpServer { get; set; }
        public int? SmtpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string AdminEmail { get; set; }
        public bool? UseSsl { get; set; }
    }
}