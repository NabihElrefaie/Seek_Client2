using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seek.Core.IRepositories.Database;

namespace Seek.API.Controllers.Database
{
    [Route("api/[controller]")]
    [ApiController]
    public class Database_Existence_CheckerController : ControllerBase
    {
        private readonly IRepo_Database_Existence_Checker _databaseChecker;
        public Database_Existence_CheckerController(IRepo_Database_Existence_Checker databaseChecker)
        {
            _databaseChecker = databaseChecker;
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckDatabase()
        {
            var result = await _databaseChecker.Database_Existence_Checker();

            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message,
                    databasePath = result.DbPath
                });
            }
            else
            {
                return NotFound(new
                {
                    message = result.Message
                });
            }
        }
    }
}
