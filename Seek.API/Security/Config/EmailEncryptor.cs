using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Seek.API.Security.Config
{
    /// <summary>
    /// Utility to create encrypted email configuration files
    /// </summary>
    public static class EmailEncryptor
    {
        /// <summary>
        /// Creates encrypted email configuration files
        /// </summary>
        /// <param name="smtpServer">SMTP server address</param>
        /// <param name="smtpPort">SMTP port</param>
        /// <param name="username">Email username</param>
        /// <param name="password">Email password</param>
        /// <param name="useSsl">Whether to use SSL</param>
        /// <param name="fromEmail">From email address</param>
        /// <param name="adminEmail">Admin email address</param>
        /// <param name="outputPath">Path to save the encrypted config file</param>
        /// <param name="keyPath">Path to save the encryption key file</param>
        public static void CreateEncryptedEmailConfig(
            string smtpServer,
            int smtpPort,
            string username,
            string password,
            bool useSsl,
            string fromEmail,
            string adminEmail,
            string outputPath,
            string keyPath)
        {
            // Create configuration content
            var configBuilder = new StringBuilder();
            configBuilder.AppendLine($"SmtpServer={smtpServer}");
            configBuilder.AppendLine($"SmtpPort={smtpPort}");
            configBuilder.AppendLine($"Username={username}");
            configBuilder.AppendLine($"Password={password}");
            configBuilder.AppendLine($"UseSsl={useSsl}");
            configBuilder.AppendLine($"FromEmail={fromEmail}");
            configBuilder.AppendLine($"AdminEmail={adminEmail}");

            // Generate a secure random encryption key
            using var aes = Aes.Create();
            aes.GenerateKey();
            byte[] keyBytes = aes.Key;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            // Encrypt the configuration
            byte[] encryptedConfig = EncryptConfig(configBuilder.ToString(), keyBytes, iv);

            // Combine IV and encrypted data
            byte[] result = new byte[iv.Length + encryptedConfig.Length];
            Array.Copy(iv, 0, result, 0, iv.Length);
            Array.Copy(encryptedConfig, 0, result, iv.Length, encryptedConfig.Length);

            // Write files
            File.WriteAllBytes(outputPath, result);
            File.WriteAllBytes(keyPath, keyBytes);

            Console.WriteLine($"Encrypted configuration written to {outputPath}");
            Console.WriteLine($"Encryption key written to {keyPath}");
        }

        private static byte[] EncryptConfig(string config, byte[] keyBytes, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] configBytes = Encoding.UTF8.GetBytes(config);
                cs.Write(configBytes, 0, configBytes.Length);
            }

            return ms.ToArray();
        }
    }
}