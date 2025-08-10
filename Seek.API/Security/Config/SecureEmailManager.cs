using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Seek.API.Security.Config
{
    /// <summary>
    /// Manages secure email configuration by reading from external encrypted files
    /// </summary>
    public class SecureEmailManager
    {
        private readonly ILogger<SecureEmailManager> _logger;
        private readonly string _configPath;
        private readonly string _keyPath;

        public SecureEmailManager(ILogger<SecureEmailManager> logger, string configPath, string keyPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
            _keyPath = keyPath ?? throw new ArgumentNullException(nameof(keyPath));
        }

        /// <summary>
        /// Reads and decrypts the email configuration file using the key file
        /// </summary>
        public string GetEmailConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    _logger.LogError("Email config file not found at: {ConfigPath}", _configPath);
                    return null;
                }

                if (!File.Exists(_keyPath))
                {
                    _logger.LogError("Email key file not found at: {KeyPath}", _keyPath);
                    return null;
                }

                // Read the encrypted configuration
                var encryptedConfig = File.ReadAllBytes(_configPath);

                // Read the encryption key
                var keyBytes = File.ReadAllBytes(_keyPath);

                // Decrypt the configuration
                var decryptedConfig = DecryptConfig(encryptedConfig, keyBytes);

                return decryptedConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read secure email configuration");
                return null;
            }
        }

        /// <summary>
        /// Decrypts the configuration using the provided key
        /// </summary>
        private string DecryptConfig(byte[] encryptedData, byte[] keyBytes)
        {
            try
            {
                using var aes = Aes.Create();

                // Derive the key from the key file
                using var sha256 = SHA256.Create();
                var key = sha256.ComputeHash(keyBytes);
                aes.Key = key;

                // The first 16 bytes of the encrypted data is the IV
                if (encryptedData.Length < 16)
                {
                    throw new InvalidOperationException("Encrypted data is invalid (too short)");
                }

                var iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, 16);
                aes.IV = iv;

                // The rest is the encrypted content
                var cipherText = new byte[encryptedData.Length - 16];
                Array.Copy(encryptedData, 16, cipherText, 0, cipherText.Length);

                // Decrypt
                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    cs.Write(cipherText, 0, cipherText.Length);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt email configuration");
                throw;
            }
        }
    }
}