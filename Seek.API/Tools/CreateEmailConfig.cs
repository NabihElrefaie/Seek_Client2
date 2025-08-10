using System;
using System.IO;
using Seek.API.Security.Config;

namespace Seek.API.Tools
{
    /// <summary>
    /// Command line tool to create encrypted email configuration files
    /// </summary>
    public class CreateEmailConfig
    {
        public static void Main2(string[] args)
        {
            try
            {
                Console.WriteLine("Email Configuration Encryption Tool");
                Console.WriteLine("==================================");

                // Default values from original appsettings.json
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string username = "ti.tickets.23@gmail.com";
                string password = "fblzdcgxntglxzcx"; // This should be updated with the current password
                bool useSsl = true;
                string fromEmail = "ti.tickets.23@gmail.com";
                string adminEmail = "Nabihabdelkhalek6@gmail.com";

                // Allow user to modify values or use defaults
                Console.WriteLine("\nPress Enter to use default values or input new values:");

                Console.Write($"SMTP Server [{smtpServer}]: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) smtpServer = input;

                Console.Write($"SMTP Port [{smtpPort}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int port)) smtpPort = port;

                Console.Write($"Username [{username}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) username = input;

                Console.Write("Password: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) password = input;

                Console.Write($"Use SSL [{useSsl}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && bool.TryParse(input, out bool ssl)) useSsl = ssl;

                Console.Write($"From Email [{fromEmail}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) fromEmail = input;

                Console.Write($"Admin Email [{adminEmail}]: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) adminEmail = input;

                // Output paths
                string appRoot = Directory.GetCurrentDirectory();
                string configPath = Path.Combine(appRoot, "email.cfg");
                string keyPath = Path.Combine(appRoot, "email.key");

                // Create encrypted configuration
                EmailEncryptor.CreateEncryptedEmailConfig(
                    smtpServer, smtpPort, username, password, useSsl, fromEmail, adminEmail, configPath, keyPath);

                Console.WriteLine("\nEmail configuration successfully created:");
                Console.WriteLine($"Configuration file: {configPath}");
                Console.WriteLine($"Key file: {keyPath}");
                Console.WriteLine("\nPlace these files in the application root directory.");

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}