using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos
{
    public class auth_register_dto
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }
}
