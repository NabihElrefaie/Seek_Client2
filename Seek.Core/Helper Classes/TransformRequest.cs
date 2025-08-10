using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Helper_Classes
{
    public class TransformRequest
    {
        /// <summary>
        /// The encryption key used for either encrypting or decrypting the database.
        /// </summary>
        public string Encryption_Key { get; set; } = string.Empty;
    }
}
