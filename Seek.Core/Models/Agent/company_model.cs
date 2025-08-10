using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Models.Agent
{
    public class company_model
    {
        [Key]
        public int Ud { get; set; }
        public string? Company_Registration_Name { get; set; }
        public string? CRN { get; set; }
        public string? VAT_Number { get; set; }
        public string? RegId { get; set; }
        public string? Company_Type { get; set; }
        public string? Tax_clients_Secret1 { get; set; }
        public string? Tax_clients_Secret2 { get; set; }
        [NotMapped]
        public int SingletonId { get; set; }
    }
}
