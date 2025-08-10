using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Seek.Core.IRepositories.Database;
using System.Security.Cryptography;
using Seek.Core.IRepositories.System;

namespace Seek.EF.Repositories.Database
{
    public class Repo_Database_Security : IRepo_Database_Security, IDisposable
    {
        private readonly ILogger<Repo_Database_Security> _logger;
        private readonly IRepo_SecureKeyManager _keyManager;
        private readonly SemaphoreSlim _dbLock = new SemaphoreSlim(1, 1);
        private readonly string _basePath;

        public Repo_Database_Security(
            ILogger<Repo_Database_Security> logger,
            IRepo_SecureKeyManager keyManager)
        {
            _logger = logger;
            _keyManager = keyManager;
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Database");
        }

        public async Task<(bool Success, string Message)> DecryptDatabaseAsync(
            string encryptedDbPath,
            string plainDbPath,
            string verificationKey)
        {
            await _dbLock.WaitAsync();
            try
            {
                // Verify the key matches our stored key
                if (!await _keyManager.VerifyKeyAsync(verificationKey))
                {
                    return (false, "Invalid verification key");
                }

                // Get the actual encryption key from key manager
                var encryptionKey = await _keyManager.GetEncryptionKeyAsync();

                // Create decryption directory if needed
                var decryptionDir = Path.GetDirectoryName(plainDbPath);
                if (!Directory.Exists(decryptionDir))
                {
                    Directory.CreateDirectory(decryptionDir);
                }

                return await DecryptInternalAsync(encryptedDbPath, plainDbPath, encryptionKey);
            }
            finally
            {
                _dbLock.Release();
            }
        }

        public async Task<(bool Success, string Message)> EncryptDatabaseAsync(
            string plainDbPath,
            string encryptedDbPath,
            string encryptionKey = null)
        {
            await _dbLock.WaitAsync();
            try
            {
                // Use provided key or fall back to stored key
                var keyToUse = encryptionKey ?? await _keyManager.GetEncryptionKeyAsync();

                // Create encryption directory if needed
                var encryptionDir = Path.GetDirectoryName(encryptedDbPath);
                if (!Directory.Exists(encryptionDir))
                {
                    Directory.CreateDirectory(encryptionDir);
                }

                return await EncryptInternalAsync(plainDbPath, encryptedDbPath, keyToUse);
            }
            finally
            {
                _dbLock.Release();
            }
        }

        private async Task<(bool Success, string Message)> DecryptInternalAsync(
            string encryptedDbPath,
            string plainDbPath,
            string encryptionKey)
        {
            try
            {
                // Verify the key first
                if (!await VerifyEncryptionKey(encryptedDbPath, encryptionKey))
                {
                    return (false, "Invalid encryption key");
                }

                using (var connection = new SqliteConnection($"Data Source={encryptedDbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        // Set the key for decryption
                        command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                        await command.ExecuteNonQueryAsync();

                        // Attach plaintext database
                        command.CommandText = $"ATTACH DATABASE '{plainDbPath}' AS plaintext KEY '';";
                        await command.ExecuteNonQueryAsync();

                        // Export decrypted data
                        command.CommandText = "SELECT sqlcipher_export('plaintext');";
                        await command.ExecuteNonQueryAsync();

                        // Detach plaintext database
                        command.CommandText = "DETACH DATABASE plaintext;";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Verify the decrypted database
                if (!await VerifyDatabaseIntegrity(plainDbPath))
                {
                    File.Delete(plainDbPath);
                    return (false, "Decryption failed - integrity check failed");
                }

                return (true, $"Database successfully decrypted to {plainDbPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database decryption failed");
                return (false, $"Decryption failed: {ex.Message}");
            }
        }

        private async Task<(bool Success, string Message)> EncryptInternalAsync(
            string plainDbPath,
            string encryptedDbPath,
            string encryptionKey)
        {
            try
            {
                // Verify the source database first
                if (!await VerifyDatabaseIntegrity(plainDbPath))
                {
                    return (false, "Source database integrity check failed");
                }

                using (var connection = new SqliteConnection($"Data Source={plainDbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        // Attach encrypted database
                        command.CommandText = $"ATTACH DATABASE '{encryptedDbPath}' AS encrypted KEY '{encryptionKey}';";
                        await command.ExecuteNonQueryAsync();

                        // Export encrypted data
                        command.CommandText = "SELECT sqlcipher_export('encrypted');";
                        await command.ExecuteNonQueryAsync();

                        // Detach encrypted database
                        command.CommandText = "DETACH DATABASE encrypted;";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Verify the encrypted database
                if (!await VerifyEncryptionKey(encryptedDbPath, encryptionKey))
                {
                    File.Delete(encryptedDbPath);
                    return (false, "Encryption verification failed");
                }

                return (true, $"Database successfully encrypted to {encryptedDbPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database encryption failed");
                return (false, $"Encryption failed: {ex.Message}");
            }
        }

        private async Task<bool> VerifyEncryptionKey(string dbPath, string key)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"PRAGMA key = '{key}';";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = "SELECT count(*) FROM sqlite_master;";
                        var result = await command.ExecuteScalarAsync();
                        return result != null && result != DBNull.Value;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> VerifyDatabaseIntegrity(string dbPath)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PRAGMA integrity_check;";
                        var result = await command.ExecuteScalarAsync() as string;
                        return result?.ToLower() == "ok";
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _dbLock?.Dispose();
        }
    
}
}