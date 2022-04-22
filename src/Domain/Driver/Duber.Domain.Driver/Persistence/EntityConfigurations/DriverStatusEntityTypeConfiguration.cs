using Duber.Domain.Driver.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Duber.Domain.Driver.Persistence.EntityConfigurations
{
    internal class DriverStatusEntityTypeConfiguration : IEntityTypeConfiguration<DriverStatus>
    {
        public void Configure(EntityTypeBuilder<DriverStatus> builder)
        {
            builder.ToTable("DriverStatuses", DriverContext.DEFAULT_SCHEMA);

            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(ct => ct.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
