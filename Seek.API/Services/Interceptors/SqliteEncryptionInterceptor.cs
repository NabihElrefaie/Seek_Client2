using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Seek.API.Services.Interceptors
{

    /// Interceptor that applies encryption settings to SQLite connections
    public class SqliteEncryptionInterceptor : DbConnectionInterceptor
    {
        private readonly IRepo_EncryptionKeyProvider _encryptionRepository;
        public SqliteEncryptionInterceptor(IRepo_EncryptionKeyProvider encryptionRepository)
        {
            _encryptionRepository = encryptionRepository;
        }

        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            _ = _encryptionRepository.ApplyEncryptionAsync(connection);
            return result;
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            await _encryptionRepository.ApplyEncryptionAsync(connection);
            return result;
        }



        /*
        private readonly string _encryptionKey;
        private readonly ILogger<SqliteEncryptionInterceptor> _logger;
        private readonly bool _allowKeyRefresh;

        /// Creates a new instance of SqliteEncryptionInterceptor
        public SqliteEncryptionInterceptor(string encryptionKey, ILogger<SqliteEncryptionInterceptor> logger, bool allowKeyRefresh = false)
        {
            _encryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _allowKeyRefresh = allowKeyRefresh;
        }

        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            ApplyEncryption(connection);
            return result;
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            ApplyEncryption(connection);
            return await ValueTask.FromResult(result);
        }

        private void ApplyEncryption(DbConnection connection)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                try
                {
                    // Check if we need to modify the connection string
                    var builder = new SqliteConnectionStringBuilder(sqliteConnection.ConnectionString);

                    // Store connection open time to handle key refresh
                    var encryptionKey = _encryptionKey;
                    var connectionOpenTime = DateTime.UtcNow;

                    // Apply encryption key using PRAGMA after opening
                    sqliteConnection.StateChange += (sender, args) =>
                    {
                        if (args.CurrentState == ConnectionState.Open)
                        {
                            // Check if we need to refresh the key (only applicable if allowed and connection was closed)
                            if (_allowKeyRefresh && args.OriginalState == ConnectionState.Closed)
                            {
                                // For production, implement a secure key refresh mechanism here
                                // This is just a placeholder for the concept
                                _logger.LogDebug("Connection opened at {time}, using current encryption key", connectionOpenTime);
                            }

                            // Apply encryption key
                            using var command = sqliteConnection.CreateCommand();
                            command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                            try
                            {
                                command.ExecuteNonQuery();
                                _logger.LogDebug("Successfully applied encryption key to database connection");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to apply encryption key to SQLite connection");
                            }
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error configuring SQLite encryption");
                }
            }
        }*/
    }

}