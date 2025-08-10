using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seek.Core.Models.Auth;


namespace Seek.EF.Configurations.Auth
{
    class auth_configs : IEntityTypeConfiguration<auth_model>
    {
        public void Configure(EntityTypeBuilder<auth_model> builder)
        {
            builder.ToTable("FK_Auth");
            //Enforce one row in the table
            builder.Property(m => m.SingletonId)
                   .HasDefaultValue(1)  // Default to 1
                   .IsRequired();
            builder.Property(m => m.HashedLogin).IsRequired();
            builder.Property(m => m.HashedPassword).IsRequired();
            builder.Property(m => m.Hashed_Refresh_Token).IsRequired();
            builder.HasIndex(m => m.HashedLogin).IsUnique();

            builder.HasIndex(m => m.SingletonId).IsUnique();
        }
    }
}
