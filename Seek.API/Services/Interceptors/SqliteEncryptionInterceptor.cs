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
    }

}