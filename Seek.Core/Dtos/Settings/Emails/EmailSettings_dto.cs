using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos.Settings.Emails
{
    /// <summary>
    /// Configuration settings for email service
    /// </summary>
    public class EmailSettings_dto
    {
        public required string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool UseSsl { get; set; }
        public required string FromEmail { get; set; }
        public required string AdminEmail { get; set; }
    }
}
