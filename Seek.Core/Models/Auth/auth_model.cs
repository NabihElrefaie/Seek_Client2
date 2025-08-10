using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Models.Auth
{
    public class auth_model
    {
        [Key]
        public int Ud { get; set; }
        [Required]
        public string? HashedLogin { get; set; }
        [Required]
        public string? HashedPassword { get; set; }
        [NotMapped]
        public string? HashedToken { get; set; }
        public string? Hashed_Refresh_Token { get; set; }
        public DateTime? ExpiresIn { get; set; }
        [NotMapped]
        public int SingletonId { get; set; }
    }
}
