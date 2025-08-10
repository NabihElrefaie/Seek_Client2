using Seek.Core.Dtos.Settings.Verification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.System
{
    public interface IRepo_VerificationManager
    {
        Task<bool> IsVerificationCompleted();
        Task<(bool IsVerified, DateTime? VerifiedAt)> GetVerificationStatusAsync();
        Task<bool> ResetVerificationAsync();
        Task<bool> SaveVerificationDataAsync(VerificationData_dto data);
        Task<bool> VerifyCodeAsync(string code);
        Task<string> GenerateVerificationCodeAsync();
    }
}
