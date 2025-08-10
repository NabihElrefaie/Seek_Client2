using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seek.API.Security;
using Seek.API.Security.New;
using Seek.Core.Dtos.Settings.Emails;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;

namespace Seek.API.Controllers.System
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VerificationController : ControllerBase
    {
        #region -> Fields
        private readonly ILogger<VerificationController> _logger;
        private readonly IRepo_VerificationManager _verification;
        private readonly IRepo_Email_Templates _email;
        private readonly SecureSettingsManager _secureSettingsManager;
        #endregion

        public VerificationController(ILogger<VerificationController> logger, IRepo_VerificationManager verificationService, IRepo_Email_Templates emailService, SecureSettingsManager secureSettingsManager)
        {
            _logger = logger;
            _verification = verificationService;
            _email = emailService;
            _secureSettingsManager = secureSettingsManager;
        }

        #region -> Endpoints
        /// Checks if the application is verified
        [HttpGet("status")]
        public async Task<IActionResult> GetVerificationStatus()
        {
            try
            {
                var (isVerified, verifiedAt) = await _verification.GetVerificationStatusAsync();
                return Ok(new { IsCompleted = isVerified, Complated_At = verifiedAt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking verification status");
                return StatusCode(500, "An error occurred while checking verification status");
            }
        }
        /// Sends a verification code to the admin email
        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode()
        {
            try
            {
                // Check if the app is already verified
                if (await _verification.IsVerificationCompleted())
                {
                    return BadRequest(new { message = "Application is already verified" });
                }

                // Generate a new verification code
                string code = await _verification.GenerateVerificationCodeAsync();

                // Get admin email from secure settings
                var emailSettings = _secureSettingsManager.GetSecureEmailSettings();
                string adminEmail = emailSettings.AdminEmail;

                if (string.IsNullOrEmpty(adminEmail))
                {
                    return BadRequest(new { message = "Admin email is not configured" });
                }

                // Send the verification code via email
                await _email.SendVerificationCodeAsync(adminEmail, code);

                return Ok(new { message = "Verification code sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code");
                return StatusCode(500, "An error occurred while sending verification code");
            }
        }
        /// Verifies the application using the provided code
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyApplication([FromBody] VerificationRequest_dto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    return BadRequest(new { message = "Verification code is required" });
                }

                bool isValid = await _verification.VerifyCodeAsync(request.Code);

                if (isValid)
                {
                    return Ok(new { message = "Application verified successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Invalid verification code or code expired" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying application");
                return StatusCode(500, "An error occurred during verification");
            }
        }
        /// Resets the verification status (requires authentication in production)
        [HttpPost("reset")]
        public async Task<IActionResult> ResetVerification()
        {
            try
            {
                // In production, this endpoint should be protected with admin authentication
                bool success = await _verification.ResetVerificationAsync();
                if (success)
                {
                    return Ok(new { message = "Verification status reset successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to reset verification status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting verification");
                return StatusCode(500, "An error occurred while resetting verification status");
            }
        }
        #endregion
    }

}
