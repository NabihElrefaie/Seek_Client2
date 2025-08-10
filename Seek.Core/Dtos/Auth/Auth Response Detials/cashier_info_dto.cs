using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos.Auth.Auth_Response
{
    public class cashier_info_dto
    {
        public string? Branch_Code { get; set; }
        public string? Name_Ar { get; set; }
        public string? Name_Eng { get; set; }
        public string? Tax_Certificate { get; set; }
        public string? Secret1 { get; set; }
        public string? Pk_Str { get; set; }
    }
}
