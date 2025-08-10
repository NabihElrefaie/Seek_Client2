using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.Database
{
        public interface IRepo_Database_Security
        {
            Task<(bool Success, string Message)> EncryptDatabaseAsync(string plainDbPath, string encryptedDbPath, string encryptionKey);
            Task<(bool Success, string Message)> DecryptDatabaseAsync(string encryptedDbPath, string plainDbPath, string encryptionKey);
        }
}
