using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seek.Core.IRepositories.Database;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using Seek.EF;
using Seek.EF.Repositories.System;
using System;
using System.Threading.Tasks;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extensions for integrating database encryption with the application startup
    /// </summary>
    public static class DatabaseEncryptionExtensions
    {
        private static readonly IRepo_VerificationManager? _verification;
        private static readonly IRepo_Email_Templates? _email;

        /// <summary>
        /// Registers database encryption services
        /// </summary>
        public static IServiceCollection AddDatabaseEncryptionServices(this IServiceCollection services)
        {
            // Register SecureKeyManager if not already registered
            services.AddSingleton(provider =>
                new SecureKeyManager(
                    Directory.GetCurrentDirectory(),
                    provider.GetRequiredService<ILogger<SecureKeyManager>>()
                )
            );


            // Register SecureSettingsManager
            services.AddSingleton(provider =>
                new SecureSettingsManager(
                    Directory.GetCurrentDirectory(),
                    provider.GetRequiredService<ILogger<SecureSettingsManager>>(),
                    provider.GetRequiredService<IConfiguration>(),
                    provider.GetRequiredService<SecureKeyManager>()
                )
            );

            // Register SqliteEncryptionInterceptor and dependencies
            services.AddScoped<IRepo_EncryptionKeyProvider, Repo_EncryptionKeyProvider>();
            services.AddScoped<Services.Interceptors.SqliteEncryptionInterceptor>();

            return services;
        }

        /// <summary>
        /// Configures application startup to check for verification before allowing database access
        /// </summary>
        public static IApplicationBuilder UseVerificationCheck(this IApplicationBuilder app)
        {



            return app.Use(async (context, next) =>
            {
                // Skip verification check for verification-related endpoints
                string path = context.Request.Path.Value?.ToLower() ?? "";

                if (path.Contains("/api/v1/verification") ||
                    path.Contains("/swagger") ||
                    path.Contains("/health"))
                {
                    await next();
                    return;
                }

                // Check verification status
                bool isVerified = await _verification.IsVerificationCompleted();

                if (!isVerified)
                {
                    // Redirect to verification status endpoint
                    context.Response.StatusCode = 403; // Forbidden
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\":\"Application requires verification\",\"verificationRequired\":true}");
                    return;
                }

                await next();
            });
        }

    }
}