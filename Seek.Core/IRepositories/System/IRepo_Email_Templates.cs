using Seek.Core.Dtos.Settings;
using Seek.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.System
{
    public interface IRepo_Email_Templates
    {
        Task<bool> SendEmailAsync(string recipient, string subject, string body);
        Task<bool> SendVerificationCodeAsync(string recipient, string verificationCode);
        Task<bool> SendNewDeviceRegistrationAsync(string deviceId, string ipAddress, string encryptionKey, string? password = null);
    }
}
