using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos.Auth.Auth_Response
{
    public class full_detail_dto
    {
        public List<company_info_dto>? Companies { get; set; }
        public List<branch_info_dto>? Branches { get; set; }
        public List<cashier_info_dto>? Pos { get; set; }
    }
}
