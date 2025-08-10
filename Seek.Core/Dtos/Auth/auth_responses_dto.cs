using Seek.Core.Dtos.Auth.Auth_Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos
{
    public class auth_responses_dto
    {
        public string? Token { get; set; }
        public string? Refresh_Token { get; set; }
        public List<full_detail_dto>? Details { get; set; }
        public DateTime? Expires_In { get; set; }
    }
}
