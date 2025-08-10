using Microsoft.Extensions.Logging;
using Seek.Core.Dtos.Settings.Emails;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Seek.API.Security.Config
{
    /// <summary>
    /// Manages secure email configuration by reading from external encrypted files
    /// </summary>
    public static class EmailConfiguration
    {
        /// <summary>
        /// Tries to configure email settings from secure files
        /// </summary>
        public static bool ConfigureFromSecureFiles(EmailSettings_dto options, ILogger logger, string configPath, string keyPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    logger.LogWarning("Email config file not found at: {ConfigPath}", configPath);
                    return false;
                }

                if (!File.Exists(keyPath))
                {
                    logger.LogWarning("Email key file not found at: {KeyPath}", keyPath);
                    return false;
                }

                // Read the encrypted configuration
                var encryptedConfig = File.ReadAllBytes(configPath);

                // Read the encryption key
                var keyBytes = File.ReadAllBytes(keyPath);

                // Decrypt the configuration
                var decryptedConfig = DecryptConfig(encryptedConfig, keyBytes, logger);

                if (string.IsNullOrEmpty(decryptedConfig))
                {
                    return false;
                }

                // Parse the config (simple key=value format for example)
                var configLines = decryptedConfig.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var configDict = configLines
                    .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains('='))
                    .ToDictionary(
                        line => line.Substring(0, line.IndexOf('=')).Trim(),
                        line => line.Substring(line.IndexOf('=') + 1).Trim()
                    );

                // Map config values to options
                if (configDict.TryGetValue("SmtpServer", out var smtpServer))
                    options.SmtpServer = smtpServer;

                if (configDict.TryGetValue("SmtpPort", out var smtpPort) && int.TryParse(smtpPort, out var port))
                    options.SmtpPort = port;

                if (configDict.TryGetValue("Username", out var username))
                    options.Username = username;

                if (configDict.TryGetValue("Password", out var password))
                    options.Password = password;

                if (configDict.TryGetValue("UseSsl", out var useSsl) && bool.TryParse(useSsl, out var ssl))
                    options.UseSsl = ssl;

                if (configDict.TryGetValue("FromEmail", out var fromEmail))
                    options.FromEmail = fromEmail;

                if (configDict.TryGetValue("AdminEmail", out var adminEmail))
                    options.AdminEmail = adminEmail;

                logger.LogInformation("Email settings loaded from secure configuration files");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load email settings from secure files");
                return false;
            }
        }

        /// <summary>
        /// Decrypts the configuration using the provided key
        /// </summary>
        private static string DecryptConfig(byte[] encryptedData, byte[] keyBytes, ILogger logger)
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
                logger.LogError(ex, "Failed to decrypt email configuration");
                return null;
            }
        }
    }
}