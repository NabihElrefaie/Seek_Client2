using Seek.EF;
using Serilog;

namespace Seek.API.Services.System
{
    public class DefaultDataService
    {
        private readonly ApplicationDbContext _context;

        public DefaultDataService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task EnsureDefaultDataAsync()
        {
            try
            {
                Log.Information("System : Creating Default Data By System");
                // Check if the database is created
                Log.Information("System : Ending Creating Default Data");

            }
            catch (TaskCanceledException ex)
            {
                Log.Error($"System : Task was canceled: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"System : An error occurred {ex.Message}");
            }
        }

    }
}
