using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.System
{
    public interface IRepo_SecureKeyManager
    {
        Task<string> GetEncryptionKeyAsync(string password = null);
        Task<bool> VerifyKeyAsync(string password);
    }
}
