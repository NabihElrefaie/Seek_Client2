using Seek.Core.Dtos.Settings.Emails;
using Seek.Core.Security;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extensions for configuring security services in application startup
    /// </summary>
    public static class StartupExtensions
    {


        /// Configures security middleware for the application
        public static IApplicationBuilder UseSeekSecurity(this IApplicationBuilder app)
        {
            // Add verification check middleware
            app.UseVerificationCheck();

            return app;
        }

        /// Initializes secure settings by transferring from appsettings to encrypted storage
        public static void InitializeSecureSettings(this IApplicationBuilder app)
        {
            // Get services
            var settingsManager = app.ApplicationServices.GetRequiredService<SecureSettingsManager>();
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            // Check if secure settings already exist
            var emailSettings = settingsManager.GetSecureEmailSettings();

            // If no secure settings exist, copy from configuration
            if (emailSettings == null || string.IsNullOrEmpty(emailSettings.SmtpServer))
            {
                emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings_dto>();
                if (emailSettings != null && !string.IsNullOrEmpty(emailSettings.SmtpServer))
                {
                    settingsManager.SaveSecureEmailSettings(emailSettings);
                }
            }
        }
    }
}