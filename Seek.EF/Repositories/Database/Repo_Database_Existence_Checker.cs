using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Seek.Core.IRepositories.Database;

namespace Seek.EF.Repositories.Database
{
    public class Repo_Database_Existence_Checker : IRepo_Database_Existence_Checker
    {
        private readonly ILogger<Repo_Database_Existence_Checker> _logger;
        private readonly IConfiguration _configuration;
        public Repo_Database_Existence_Checker(ILogger<Repo_Database_Existence_Checker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<(bool Success, string Message, string DbPath)> Database_Existence_Checker()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("SQLite : Connection string is not configured");
                return (false, "SQLite connection string is not configured.", null ?? "");
            }

            var dataSourcePrefix = "Data Source=";
            var startIndex = connectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1)
            {
                _logger.LogError("SQLite : Data Source not found in connection string");
                return (false, "Data Source not found in connection string.", null ?? "");
            }

            startIndex += dataSourcePrefix.Length;
            var endIndex = connectionString.IndexOf(';', startIndex);
            var dbPath = endIndex == -1
                ? connectionString.Substring(startIndex).Trim()
                : connectionString.Substring(startIndex, endIndex - startIndex).Trim();

            if (string.IsNullOrEmpty(dbPath))
            {
                _logger.LogError("SQLite : Database path is empty");
                return (false, "Database path is empty.", null??"");
            }

            bool exists = await Task.Run(() => File.Exists(dbPath));

            if (exists)
            {
                _logger.LogInformation($"Database file exists at {dbPath}");
                return (true, "Database exists.", dbPath);
            }
            else
            {
                _logger.LogWarning($"Database file does not exist at {dbPath}");
                return (false, "Database does not exist.", dbPath);
            }
        }
    }
}
