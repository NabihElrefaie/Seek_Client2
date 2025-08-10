using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Repositories.System
{
    public class Repo_SecureKeyManager : IRepo_SecureKeyManager
    {
        private readonly SecureKeyManager _secureKeyManager;

        public Repo_SecureKeyManager(SecureKeyManager secureKeyManager)
        {
            _secureKeyManager = secureKeyManager ?? throw new ArgumentNullException(nameof(secureKeyManager));
        }

        public Task<string> GetEncryptionKeyAsync(string? password = null)
        {
            return Task.FromResult(_secureKeyManager.GetEncryptionKey(password));
        }

        public Task<bool> VerifyKeyAsync(string password)
        {
            return Task.FromResult(_secureKeyManager.ValidatePassword(password));
        }
    }
}
