using Microsoft.Extensions.Logging;
using Seek.Core.Dtos.Settings.Verification;
using Seek.Core.IRepositories.System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Seek.EF.Repositories.System
{
    public class Repo_VerificationManager : IRepo_VerificationManager
    {
        #region -> Fields
        private readonly ILogger<Repo_VerificationManager> _logger;
        private readonly string _verificationFilePath;
        private const int VerificationCodeLength = 6;
        private const int ExpirationMinutes = 30;
        private readonly string _workingDirectory;
        #endregion

        public Repo_VerificationManager(ILogger<Repo_VerificationManager> logger)
        {
            _workingDirectory = Directory.GetCurrentDirectory();
            _verificationFilePath = Path.Combine(_workingDirectory, "verification_status.json");
            _logger = logger;
            EnsureVerificationStatusFile();
            _logger.LogInformation(message: "System : Verification Manager initialized");
        }

        #region-> Interface Implementation
        /// Generates a new verification code and stores its hash
        public async Task<string> GenerateVerificationCodeAsync()
        {
            try
            {
                // Generate a random code
                string code = GenerateRandomCode(VerificationCodeLength);

                // Store the hash of the code
                var verificationData = new VerificationData_dto
                {
                    CodeHash = HashVerificationCode(code),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
                    IsVerified = false,
                    VerifiedAt = null
                };

                await SaveVerificationDataAsync(verificationData);
                _logger.LogInformation(message: $"System : Verification code generated successfully Valid Until {DateTime.UtcNow.AddMinutes(ExpirationMinutes)}");

                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate verification code");
                throw;
            }
        }
        /// Verifies if the provided code matches the stored hash
        public async Task<bool> VerifyCodeAsync(string code)
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();

                // If already verified, return true
                if (verificationData.IsVerified)
                {
                    return true;
                }

                // Check if expired
                if (DateTime.UtcNow > verificationData.ExpiresAt)
                {
                    _logger.LogWarning("System : Verification code expired");
                    return false;
                }

                // Verify the code
                string codeHash = HashVerificationCode(code);
                bool isValid = codeHash == verificationData.CodeHash;

                if (isValid)
                {
                    // Update verification status
                    verificationData.IsVerified = true;
                    verificationData.VerifiedAt = DateTime.UtcNow;
                    await SaveVerificationDataAsync(verificationData);
                    _logger.LogInformation("System : Application verification successful");
                }
                else
                {
                    _logger.LogWarning("System : Invalid verification code provided");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System : Failed to verify code");
                throw;
            }
        }
        /// Gets the verification status details
        public async Task<(bool IsVerified, DateTime? VerifiedAt)> GetVerificationStatusAsync()
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();
                return (verificationData.IsVerified, verificationData.VerifiedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get verification status");
                return (false, null);
            }
        }
        public async Task<bool> IsVerificationCompleted()
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();
                return verificationData.IsVerified;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check verification status");
                return false;
            }
        }
        public async Task<bool> SaveVerificationDataAsync(VerificationData_dto data)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(data);
                await File.WriteAllTextAsync(_verificationFilePath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save verification data.");
                return false;
            }
        }
        /// Resets the verification status, requiring a new verification
        public async Task<bool> ResetVerificationAsync()
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();
                verificationData.IsVerified = false;
                verificationData.VerifiedAt = null;
                verificationData.CodeHash = null;
                verificationData.ExpiresAt = DateTime.MinValue;
                await SaveVerificationDataAsync(verificationData);
                _logger.LogInformation("System : Verification status reset successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset verification status");
                return false;
            }
        }
        #endregion

        #region-> Private Helper Methods
        private string GenerateRandomCode(int length)
        {
            // Generate a numeric code of specified length
            StringBuilder code = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(data);
                    int value = BitConverter.ToInt32(data, 0);
                    code.Append(Math.Abs(value % 10)); // Get a digit (0-9)
                }
            }
            return code.ToString();
        }
        private string HashVerificationCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            using (var sha = SHA256.Create())
            {
                byte[] textData = Encoding.UTF8.GetBytes(code);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        private void EnsureVerificationStatusFile()
        {
            if (!File.Exists(_verificationFilePath))
            {
                var initialData = new VerificationData_dto
                {
                    CodeHash = null,
                    ExpiresAt = DateTime.MinValue,
                    IsVerified = false,
                    VerifiedAt = null
                };

                string jsonString = JsonSerializer.Serialize(initialData);
                File.WriteAllText(_verificationFilePath, jsonString);
                _logger.LogInformation("System : Created new verification status file");
            }
        }
        private async Task<VerificationData_dto> LoadVerificationDataAsync()
        {
            try
            {
                EnsureVerificationStatusFile();
                var json = await File.ReadAllTextAsync(_verificationFilePath);
                return JsonSerializer.Deserialize<VerificationData_dto>(json)
                       ?? new VerificationData_dto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load verification data.");
                return new VerificationData_dto();
            }
        }
        #endregion
    }
}
