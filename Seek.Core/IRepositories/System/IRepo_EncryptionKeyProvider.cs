using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.System
{
    public interface IRepo_EncryptionKeyProvider
    {
        Task ApplyEncryptionAsync(DbConnection connection);

        string GetEncryptionKey(string? userPassword = null);
    }
}
