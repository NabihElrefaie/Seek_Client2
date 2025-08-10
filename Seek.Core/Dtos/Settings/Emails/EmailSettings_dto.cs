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
        public  string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public  string Username { get; set; }
        public  string Password { get; set; }
        public bool UseSsl { get; set; }
        public  string FromEmail { get; set; }
        public  string AdminEmail { get; set; }
    }
}
