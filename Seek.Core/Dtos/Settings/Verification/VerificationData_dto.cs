using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos.Settings.Verification
{
    public  class VerificationData_dto
    {
        public string? CodeHash { get; set; } = null;
        public DateTime ExpiresAt { get; set; } = DateTime.MinValue;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; } = null;
    }
}
