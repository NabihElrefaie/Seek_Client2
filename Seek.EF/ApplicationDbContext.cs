using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Seek.Core.Models.Agent;
using Seek.Core.Models.Auth;
using Seek.EF.Configurations.Agent;
using Seek.EF.Configurations.Auth;

using System.Data;


namespace Seek.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Apply configurations
            new auth_configs().Configure(modelBuilder.Entity<auth_model>());
            new company_configs().Configure(modelBuilder.Entity<company_model>());
            new branch_configs().Configure(modelBuilder.Entity<branch_model>());
            new cashier_configs().Configure(modelBuilder.Entity<cashier_model>());

        }
        // Ensure connection is closed when context is disposed
        public override void Dispose()
        {
            var connection = Database.GetDbConnection();
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
                connection.Dispose();
            }
            base.Dispose();
        }
    }
}
