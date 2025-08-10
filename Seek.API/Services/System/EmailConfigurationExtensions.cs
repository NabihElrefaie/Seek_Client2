using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seek.API.Security.Config;
using Seek.Core.Dtos.Settings.Emails;
using System;
using System.IO;

namespace Seek.API.Services.System
{
    public static class EmailConfigurationExtensions
    {
        public static void ConfigureEmailSettings(this IServiceCollection services, IServiceProvider serviceProvider = null)
        {
            services.Configure<EmailSettings_dto>(options =>
            {
                // Create a logger factory for the email manager
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });

                var logger = loggerFactory.CreateLogger("EmailConfig");

                // Get paths to email config files (in same directory as app by default)
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "email.cfg");
                string keyPath = Path.Combine(Directory.GetCurrentDirectory(), "email.key");

                // Load settings from secure files
                if (!EmailConfiguration.ConfigureFromSecureFiles(options, logger, configPath, keyPath))
                {
                    // If we cannot load from encrypted files, log an error
                    logger.LogError("Failed to load email settings from encrypted files. Email functionality may not work.");

                    // Set default values to prevent null reference exceptions
                    options.SmtpServer = "localhost";
                    options.SmtpPort = 25;
                    options.Username = "";
                    options.Password = "";
                    options.UseSsl = false;
                    options.FromEmail = "no-reply@example.com";
                    options.AdminEmail = "admin@example.com";
                }
            });
        }
    }
}