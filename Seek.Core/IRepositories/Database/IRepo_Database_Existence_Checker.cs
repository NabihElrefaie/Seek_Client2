using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.Database
{
    public interface IRepo_Database_Existence_Checker
    {
        Task<(bool Success, string Message, string DbPath)> Database_Existence_Checker();

    }
}
