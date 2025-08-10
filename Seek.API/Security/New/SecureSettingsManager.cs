using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Seek.Core.Security;
using Seek.Core.Dtos.Settings.Emails;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Manages secure storage and retrieval of sensitive application settings
    /// </summary>
    public class SecureSettingsManager
    {
        private readonly string _applicationPath;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly SecureKeyManager _keyManager;
        private const string SecureSettingsFileName = "secure_settings.dat";

        public SecureSettingsManager(string applicationPath, ILogger logger, IConfiguration configuration, SecureKeyManager keyManager)
        {
            _applicationPath = applicationPath ?? throw new ArgumentNullException(nameof(applicationPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));
        }

        /// <summary>
        /// Gets secure email settings, either from encrypted storage or falls back to configuration
        /// </summary>
        public EmailSettings_dto GetSecureEmailSettings()
        {
            try
            {
                // Try to load from secure storage first
                var secureSettings = LoadSecureSettings();
                if (secureSettings != null)
                {
                    _logger.LogInformation("Email settings loaded from secure storage");
                    return secureSettings.EmailSettings;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load email settings from secure storage, falling back to configuration");
            }

            // Fall back to appsettings.json
            return _configuration.GetSection("EmailSettings").Get<EmailSettings_dto>();
        }

        /// <summary>
        /// Saves email settings to secure encrypted storage
        /// </summary>
        public bool SaveSecureEmailSettings(EmailSettings_dto emailSettings)
        {
            try
            {
                var secureSettings = LoadSecureSettings() ?? new SecureApplicationSettings();
                secureSettings.EmailSettings = emailSettings;

                return SaveSecureSettings(secureSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save email settings to secure storage");
                return false;
            }
        }

        /// <summary>
        /// Loads secure settings from encrypted storage
        /// </summary>
        private SecureApplicationSettings LoadSecureSettings()
        {
            string filePath = Path.Combine(_applicationPath, SecureSettingsFileName);
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                // Get encryption key from SecureKeyManager
                string encryptionKey = _keyManager.GetEncryptionKey();

                // Read and decrypt the file
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string jsonData = DecryptData(encryptedData, encryptionKey);

                // Deserialize settings
                return JsonConvert.DeserializeObject<SecureApplicationSettings>(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load secure settings");
                throw;
            }
        }

        /// <summary>
        /// Saves secure settings to encrypted storage
        /// </summary>
        private bool SaveSecureSettings(SecureApplicationSettings settings)
        {
            try
            {
                string filePath = Path.Combine(_applicationPath, SecureSettingsFileName);

                // Get encryption key from SecureKeyManager
                string encryptionKey = _keyManager.GetEncryptionKey();

                // Serialize and encrypt the settings
                string jsonData = JsonConvert.SerializeObject(settings);
                byte[] encryptedData = EncryptData(jsonData, encryptionKey);

                // Write to file
                File.WriteAllBytes(filePath, encryptedData);

                _logger.LogInformation("Secure settings saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save secure settings");
                return false;
            }
        }

        /// <summary>
        /// Encrypts data using AES with the provided key
        /// </summary>
        private byte[] EncryptData(string data, string key)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Convert.FromBase64String(key);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // First write the IV
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(dataBytes, 0, dataBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts data using AES with the provided key
        /// </summary>
        private string DecryptData(byte[] encryptedData, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;

                // Get the IV from the beginning of the encrypted data
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(
                        new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length),
                        decryptor, CryptoStreamMode.Read))
                    {
                        cs.CopyTo(ms);
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }

    /// <summary>
    /// Class to hold all secure application settings
    /// </summary>
    public class SecureApplicationSettings
    {
        public EmailSettings_dto EmailSettings { get; set; }
    }
}