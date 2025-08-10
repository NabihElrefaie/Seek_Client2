using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Models.Agent
{
    public class cashier_model
    {
        [Key]
        public int Ud { get; set; }
        public string? Branch_Code { get; set; }
        public string? Name_Ar { get; set; }
        public string? Name_Eng { get; set; }
        public string? Tax_Certificate { get; set; }
        public string? Secret1 { get; set; }
        public string? Pk_Str { get; set; }
        [NotMapped]
        public int SingletonId { get; set; }
    }
}
