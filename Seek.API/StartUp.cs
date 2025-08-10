using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seek.API.Security.New;
using Seek.API.Services.Interceptors;
using Seek.API.Services.System;
using Seek.Core;
using Seek.Core.Dtos.Settings.Emails;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using Seek.EF;
using Seek.EF.Repositories.System;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Data.Common;

namespace Seek.API
{
    public class StartUp
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public StartUp(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Initialize SQLCipher
            InitializeSqlCipher();
            // 2. Configure database services with encryption
            ConfigureDatabaseServices(services);

            // Configure email settings from encrypted config files
            services.ConfigureEmailSettings();

            // Register repositories and other services
            services.AddRepositoryServices();

            // Other framework services Mapping
            services.AddAutoMapper(typeof(Mappings).Assembly);
            // Adding Defaults Data Values
            services.AddScoped<DefaultDataService>();

            // Add API versioning
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            // Configure Swagger
            services.AddVersionedApiExplorer(c => c.GroupNameFormat = "'v'VVV");
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            // Add controllers
            services.AddControllers();
        }
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            // 1. Initialize secure settings
            app.InitializeSecureSettings();

            // 2. Initialize database before anything else
            InitializeDatabase(app);

            // 3. Add verification middleware (checks if app is verified)
            app.UseSeekSecurity();

            // Configure environment-specific middleware
            if (_environment.IsDevelopment())
            {
                Log.Information("System : Starting in development environment");
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    foreach (var desc in provider.ApiVersionDescriptions)
                    {
                        c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
                    }
                });
            }
            else
            {
                Log.Information("System : Starting in Production environment");
            }
            // Common middleware
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #region Database Configuration
        private void InitializeSqlCipher()
        {
            SQLitePCL.Batteries_V2.Init();
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlcipher());
        }
        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            // Register SecureKeyManager as a singleton (if not already registered)
            if (services.BuildServiceProvider().GetService<SecureKeyManager>() == null)
            {
                services.AddSingleton<SecureKeyManager>(provider =>
                new SecureKeyManager(
                    Directory.GetCurrentDirectory(),
                    provider.GetRequiredService<ILogger<SecureKeyManager>>()
                )
            );
            }
            // Register VerificationService (if not already registered)
            if (services.BuildServiceProvider().GetService<Repo_VerificationManager>() == null)
            {
                services.AddSingleton<Repo_VerificationManager>(provider =>
                    new Repo_VerificationManager(
                        provider.GetRequiredService<ILogger<Repo_VerificationManager>>()
                    )
                );
            }

            var connectionString = GetConnectionString();
            // Ensure database directory exists
            EnsureDatabaseDirectory();

            // Register DbContext with encryption
            services.AddDbContext<ApplicationDbContext>(
                options => ConfigureDbContextOptions(options, connectionString, services)
                );
        }
        private string GetEncryptionKey()
        {
            var encryptionKey = _configuration["EncryptionKey"];
            if (string.IsNullOrWhiteSpace(encryptionKey))
            {
                // Try to get the key from the secure key manager if DI is available
                try
                {
                    // Create a temporary service provider if needed
                    using (var scope = new ServiceCollection()
                        .AddLogging()
                        .Configure<EmailSettings_dto>(options =>
                        {
                            // Create logger to use in configuration
                            using var loggerFactory = LoggerFactory.Create(builder =>
                            {
                                builder.AddConsole();
                                builder.AddDebug();
                            });
                            var logger = loggerFactory.CreateLogger("EmailConfig");

                            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "email.cfg");
                            string keyPath = Path.Combine(Directory.GetCurrentDirectory(), "email.key");

                            // Load from secure files
                            if (!Security.Config.EmailConfiguration.ConfigureFromSecureFiles(options, logger, configPath, keyPath))
                            {
                                logger.LogError("Failed to load email settings from encrypted files. Email functionality may not work.");
                            }
                        })
                        .AddSingleton<IRepo_Email_Templates>()
                        .AddSingleton<SecureKeyManager>(provider =>
                            new SecureKeyManager(
                                Directory.GetCurrentDirectory(),
                                provider.GetRequiredService<ILogger<SecureKeyManager>>(),
                                provider.GetService<IRepo_Email_Templates>()
                            ))
                        .BuildServiceProvider())
                    {
                        var keyManager = scope.GetRequiredService<SecureKeyManager>();
                        var userPassword = _configuration["UserPassword"];
                        encryptionKey = keyManager.GetEncryptionKey(userPassword);
                        Log.Information("SQLite: Using securely generated encryption key");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "SQLite encryption key is not configured and secure key manager failed");
                    throw new InvalidOperationException("SQLite encryption key is not configured. Set the 'EncryptionKey' in configuration.");
                }
            }
            return encryptionKey;
        }
        private string GetConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Log.Error("SQLite connection string is not configured");
                throw new InvalidOperationException("SQLite connection string is not configured. Set the 'DefaultConnection' in configuration.");
            }
            return connectionString;
        }
        private void EnsureDatabaseDirectory()
        {
            var projectRoot = Directory.GetCurrentDirectory();
            var dbFolderPath = Path.Combine(projectRoot, "Database");
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
                Log.Information("SQLite : Database directory created at {DbFolderPath}", dbFolderPath);
            }
        }
        private void ConfigureDbContextOptions(DbContextOptionsBuilder options, string connectionString, IServiceCollection services)
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqliteOptions.CommandTimeout(600);
            });
            // Add the encryption interceptor
            var interceptor = services.BuildServiceProvider().GetRequiredService<SqliteEncryptionInterceptor>();
            options.AddInterceptors(interceptor);
        }
        #endregion

        #region Database Initialization
        private string GetDatabasePath(string connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }
        private void CleanupDatabaseConnections()
        {
            SqliteConnection.ClearAllPools();
            DbContextManager.DisposeAll();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void InitializeDatabase(IApplicationBuilder app)
        {
            const int maxRetries = 3;
            var encryptionKey = GetEncryptionKey();
            var connectionString = GetConnectionString();
            var dbPath = GetDatabasePath(connectionString);

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    // Ensure clean state before operations
                    CleanupDatabaseConnections();

                    // Handle encryption status
                    if (!EnsureDatabaseEncryption(dbPath, encryptionKey))
                    {
                        throw new InvalidOperationException("Failed to ensure database encryption");
                    }

                    // Apply migrations and seed data
                    using var scope = app.ApplicationServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Apply migrations
                    dbContext.Database.Migrate();

                    // Seed data
                    var defaultDataService = scope.ServiceProvider.GetRequiredService<DefaultDataService>();
                    defaultDataService.EnsureDefaultDataAsync().GetAwaiter().GetResult();

                    Log.Information("SQLite : Database initialized successfully");
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"SQLite : Database initialization failed (attempt {retryCount + 1}/{maxRetries})");
                    if (retryCount >= maxRetries - 1)
                        throw;

                    // Add some delay before retrying
                    Thread.Sleep(1000 * (int)Math.Pow(2, retryCount + 1));  // Exponential backoff
                }
            }
        }
        private bool EnsureDatabaseEncryption(string dbPath, string encryptionKey)
        {
            if (!File.Exists(dbPath))
            {
                Log.Information("SQLite : Creating new encrypted database");
                return CreateNewEncryptedDatabase(dbPath, encryptionKey);
            }

            if (IsDatabaseEncryptedAndValid(dbPath, encryptionKey))
            {
                Log.Information("SQLite : Database is properly encrypted");
                return true;
            }

            Log.Warning("SQLite : Database needs encryption");
            return EncryptExistingDatabase(dbPath, encryptionKey);
        }
        private bool CreateNewEncryptedDatabase(string dbPath, string encryptionKey)
        {
            try
            {
                CleanupDatabaseConnections();

                // Ensure parent directory exists
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // Delete existing file if it exists
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }

                // Create and configure the new encrypted database
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA key = '{encryptionKey}';";
                cmd.ExecuteNonQuery();

                // Verify encryption by creating/dropping test table
                cmd.CommandText = "CREATE TABLE encryption_test (id INTEGER PRIMARY KEY); DROP TABLE encryption_test;";
                cmd.ExecuteNonQuery();

                Log.Information("SQLite : Successfully created new encrypted database");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SQLite : Failed to create new encrypted database");
                return false;
            }
        }
        private bool EncryptExistingDatabase(string dbPath, string encryptionKey)
        {
            // Create a temp directory for our operations
            var tempFolder = Path.Combine(Path.GetDirectoryName(dbPath), "Temp");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            var tempDbPath = Path.Combine(tempFolder, Path.GetRandomFileName() + ".db");
            var backupPath = Path.Combine(tempFolder, $"backup_{DateTime.Now:yyyyMMddHHmmss}.db");

            try
            {
                CleanupDatabaseConnections();

                // Step 1: Create backup of original database
                File.Copy(dbPath, backupPath, true);
                Log.Information($"SQLite : Created backup of original database at {backupPath}");

                // Step 2: Create new encrypted database
                if (!CreateNewEncryptedDatabase(tempDbPath, encryptionKey))
                {
                    Log.Error("SQLite : Failed to create temporary encrypted database");
                    return false;
                }

                // Step 3: Copy data from original to encrypted database
                if (!MigrateDataToEncryptedDatabase(dbPath, tempDbPath, encryptionKey))
                {
                    Log.Error("SQLite : Failed to migrate data to encrypted database");
                    return false;
                }

                // Step 4: Replace original with encrypted database
                if (!ReplaceWithEncryptedDatabase(tempDbPath, dbPath))
                {
                    Log.Error("SQLite : Failed to replace original database with encrypted one");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SQLite : Failed to encrypt existing database");
                return false;
            }
            finally
            {
                // Cleanup temporary file
                if (File.Exists(tempDbPath))
                {
                    try { File.Delete(tempDbPath); } catch { }
                }
            }
        }
        private bool MigrateDataToEncryptedDatabase(string sourceDbPath, string encryptedDbPath, string encryptionKey)
        {
            try
            {
                // Connect to the source (unencrypted) database
                using (var sourceConn = new SqliteConnection($"Data Source={sourceDbPath}"))
                {
                    sourceConn.Open();

                    // First, gather all table names
                    List<string> tables = new List<string>();
                    using (var tablesCmd = sourceConn.CreateCommand())
                    {
                        tablesCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
                        using (var reader = tablesCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tables.Add(reader.GetString(0));
                            }
                        }
                    }

                    // Now connect to the destination database
                    using (var destConn = new SqliteConnection($"Data Source={encryptedDbPath}"))
                    {
                        destConn.Open();

                        // Set encryption key
                        using (var destCmd = destConn.CreateCommand())
                        {
                            destCmd.CommandText = $"PRAGMA key = '{encryptionKey}';";
                            destCmd.ExecuteNonQuery();
                        }

                        // For each table, get the create SQL and copy data in separate commands
                        foreach (var table in tables)
                        {
                            string createTableSql;
                            using (var schemaCmd = sourceConn.CreateCommand())
                            {
                                schemaCmd.CommandText = $"SELECT sql FROM sqlite_master WHERE type='table' AND name='{table}';";
                                createTableSql = schemaCmd.ExecuteScalar() as string;
                            }

                            if (!string.IsNullOrEmpty(createTableSql))
                            {
                                // Create table in destination
                                using (var createCmd = destConn.CreateCommand())
                                {
                                    createCmd.CommandText = createTableSql;
                                    createCmd.ExecuteNonQuery();
                                }

                                // Copy data
                                CopyTableData(sourceConn, destConn, table);
                            }
                        }
                    }
                }

                Log.Information("SQLite : Successfully migrated data to encrypted database");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SQLite : Failed to migrate data to encrypted database");
                return false;
            }
        }
        private void CopyTableData(SqliteConnection sourceConn, SqliteConnection destConn, string tableName)
        {
            try
            {
                // Get column names in a separate command and connection scope
                List<string> columns = new List<string>();
                using (var colCmd = sourceConn.CreateCommand())
                {
                    colCmd.CommandText = $"PRAGMA table_info({tableName});";
                    using (var colReader = colCmd.ExecuteReader())
                    {
                        while (colReader.Read())
                        {
                            columns.Add(colReader.GetString(1)); // Column name is at index 1
                        }
                    }
                }

                if (columns.Count > 0)
                {
                    // Get data from source in a separate command
                    var columnsStr = string.Join(", ", columns);
                    using (var dataCmd = sourceConn.CreateCommand())
                    {
                        dataCmd.CommandText = $"SELECT {columnsStr} FROM {tableName};";

                        // Begin transaction in destination for faster inserts
                        using (var transaction = destConn.BeginTransaction())
                        {
                            using (var dataReader = dataCmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    // Create a new command for each insert to avoid reader conflicts
                                    using (var insertCmd = destConn.CreateCommand())
                                    {
                                        insertCmd.Transaction = transaction;

                                        var parameters = new List<string>();
                                        for (int i = 0; i < columns.Count; i++)
                                        {
                                            parameters.Add($"@p{i}");
                                            var param = insertCmd.CreateParameter();
                                            param.ParameterName = $"@p{i}";
                                            param.Value = dataReader.IsDBNull(i) ? DBNull.Value : dataReader.GetValue(i);
                                            insertCmd.Parameters.Add(param);
                                        }

                                        insertCmd.CommandText = $"INSERT INTO {tableName} ({columnsStr}) VALUES ({string.Join(", ", parameters)});";
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to copy data for table {tableName}");
                throw;
            }
        }
        private bool ReplaceWithEncryptedDatabase(string encryptedDbPath, string originalDbPath)
        {
            const int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    CleanupDatabaseConnections();

                    // Ensure no connections are open
                    SqliteConnection.ClearAllPools();

                    if (File.Exists(originalDbPath))
                    {
                        File.Delete(originalDbPath);
                    }

                    File.Copy(encryptedDbPath, originalDbPath);

                    Log.Information($"SQLite : Successfully replaced original database with encrypted one");
                    return true;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    Log.Warning($"SQLite : Failed to replace database file, retrying in {1000 * (i + 1)}ms");
                    Thread.Sleep(1000 * (i + 1));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "SQLite : Failed to replace database file");
                    return false;
                }
            }

            return false;
        }
        private bool IsDatabaseEncryptedAndValid(string dbPath, string encryptionKey)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA key = '{encryptionKey}';";
                cmd.ExecuteNonQuery();

                // Test with a simple query
                cmd.CommandText = "SELECT count(*) FROM sqlite_master;";
                cmd.ExecuteScalar();

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        // Example: a static list of active DbContexts
        public static class DbContextManager
        {
            private static readonly List<WeakReference<ApplicationDbContext>> _contexts = new();
            private static readonly object _lock = new();

            public static void Register(ApplicationDbContext context)
            {
                lock (_lock)
                {
                    _contexts.Add(new WeakReference<ApplicationDbContext>(context));
                    // Ensure we close connections when context is disposed
                    context.Database.GetDbConnection().StateChange += (sender, e) =>
                    {
                        if (e.CurrentState == ConnectionState.Closed)
                        {
                            var connection = (DbConnection)sender;
                            connection.Dispose();
                        }
                    };
                }
            }

            public static void DisposeAll()
            {
                lock (_lock)
                {
                    foreach (var contextRef in _contexts.ToList())
                    {
                        if (contextRef.TryGetTarget(out var context))
                        {
                            try
                            {
                                // Explicitly close and dispose the connection
                                var connection = context.Database.GetDbConnection();
                                if (connection.State != ConnectionState.Closed)
                                {
                                    connection.Close();
                                    connection.Dispose();
                                }
                                context.Dispose();
                            }
                            catch { }
                        }
                    }
                    _contexts.Clear();
                }

                // Additional cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
