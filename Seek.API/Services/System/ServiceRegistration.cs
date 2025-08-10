using Seek.API.Security.New;
using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories.Database;
using Seek.Core.IRepositories.System;
using Seek.EF.Repositories.Database;
using Seek.EF.Repositories.System;

namespace Seek.API.Services.System
{
    public static class ServiceRegistration
    {
        public static void AddRepositoryServices(this IServiceCollection services)
        {
            // Services
            services.AddCors();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Register security and encryption services
            services.AddDatabaseEncryptionServices();

            // Singleton services - Note: SecureKeyManager & VerificationManager are registered in StartUp.cs
            // to ensure proper initialization with correct dependencies

            // Scoped repositories
            services.AddScoped<IRepo_Database_Security, Repo_Database_Security>();
            services.AddScoped<IRepo_Database_Existence_Checker, Repo_Database_Existence_Checker>();
            services.AddScoped<IRepo_Email_Templates, Repo_Email_Templates>();

        }
    }
}
