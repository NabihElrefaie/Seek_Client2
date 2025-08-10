using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using sun.security.krb5;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Repositories.System
{
    public  class Repo_EncryptionKeyProvider : IRepo_EncryptionKeyProvider
    {
        private readonly SecureKeyManager _keyManager;
        private readonly string _fallbackKey;
        private readonly ILogger<Repo_EncryptionKeyProvider> _logger;
        private readonly bool _allowKeyRefresh;

        public Repo_EncryptionKeyProvider(SecureKeyManager keyManager, string fallbackKey, ILogger<Repo_EncryptionKeyProvider> logger)
        {
            _keyManager = keyManager;
            _fallbackKey = fallbackKey;
            _logger = logger;
        }
        public string GetEncryptionKey(string? userPassword = null)
        {
            try
            {
                _logger.LogInformation("System : Using securely stored encryption key");
                return _keyManager.GetEncryptionKey(userPassword);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get key from secure manager, using fallback key");
                return _fallbackKey;
            }
        }
        public async Task ApplyEncryptionAsync(DbConnection connection)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                try
                {
                    var encryptionKey = _keyManager.GetEncryptionKey();
                    var connectionOpenTime = DateTime.UtcNow;

                    if (sqliteConnection.State == ConnectionState.Open)
                    {
                        await ApplyKeyAsync(sqliteConnection, encryptionKey);
                    }
                    else
                    {
                        var tcs = new TaskCompletionSource();

                        void OnStateChange(object sender, StateChangeEventArgs args)
                        {
                            if (args.CurrentState == ConnectionState.Open)
                            {
                                if (_allowKeyRefresh && args.OriginalState == ConnectionState.Closed)
                                {
                                    _logger.LogDebug("Connection opened at {time}, using current encryption key", connectionOpenTime);
                                }

                                _ = ApplyKeyAsync(sqliteConnection, encryptionKey)
                                    .ContinueWith(t =>
                                    {
                                        if (t.Exception != null)
                                            _logger.LogError(t.Exception, "Failed to apply encryption key asynchronously");
                                        tcs.SetResult();
                                    });

                                sqliteConnection.StateChange -= OnStateChange;
                            }
                        }

                        sqliteConnection.StateChange += OnStateChange;

                        await tcs.Task;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error configuring SQLite encryption");
                }
            }
            else
            {
                _logger.LogWarning("Connection is not a SqliteConnection");
            }
        }

        private async Task ApplyKeyAsync(SqliteConnection connection, string encryptionKey)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA key = '{encryptionKey}';";
            await command.ExecuteNonQueryAsync();
            _logger.LogDebug("Successfully applied encryption key to database connection");
        }
    }
}
