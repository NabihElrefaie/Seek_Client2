using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seek.Core.Models.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Configurations.Agent
{
    class cashier_configs : IEntityTypeConfiguration<cashier_model>
    {
        public void Configure(EntityTypeBuilder<cashier_model> builder)
        {
            builder.ToTable("FK_Cashier");
            //Enforce one row in the table
            builder.Property(m => m.SingletonId)
                   .HasDefaultValue(1)  // Default to 1
                   .IsRequired();
            builder.HasIndex(m => m.SingletonId).IsUnique();
        }
    }
}
