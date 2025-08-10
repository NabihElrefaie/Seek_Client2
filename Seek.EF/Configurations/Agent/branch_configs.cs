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
    class branch_configs : IEntityTypeConfiguration<branch_model>
    {
        public void Configure(EntityTypeBuilder<branch_model> builder)
        {
            builder.ToTable("FK_Branch");
            //Enforce one row in the table
            builder.Property(m => m.SingletonId)
                   .HasDefaultValue(1)  // Default to 1
                   .IsRequired();
            builder.HasIndex(m => m.SingletonId).IsUnique();
        }
    }
}
