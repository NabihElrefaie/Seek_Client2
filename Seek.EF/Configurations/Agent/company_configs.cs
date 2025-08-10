using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seek.Core.Models.Agent;
using Seek.Core.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Configurations.Agent
{
    class company_configs : IEntityTypeConfiguration<company_model>
    {
        public void Configure(EntityTypeBuilder<company_model> builder)
        {
            builder.ToTable("FK_Company");
            //Enforce one row in the table
            builder.Property(m => m.SingletonId)
                   .HasDefaultValue(1)  // Default to 1
                   .IsRequired();
            builder.HasIndex(m => m.SingletonId).IsUnique();

        }
    }
}
