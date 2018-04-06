using Duber.Domain.Driver.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Duber.Domain.Driver.Persistence.EntityConfigurations
{
    internal class VehicleEntityTypeConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles", DriverContext.DEFAULT_SCHEMA);

            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.DomainEvents);

            builder.Property(b => b.Id)
                .ForSqlServerUseSequenceHiLo("vehicleseq", DriverContext.DEFAULT_SCHEMA);

            builder.Property(b => b.Active)
                .IsRequired();

            builder.Property(b => b.Brand)
                .IsRequired();

            builder.Property(b => b.Model)
                .IsRequired();

            builder.Property(b => b.Plate)
                .IsRequired();

            builder.Property<int>("TypeId").IsRequired();
            builder.Property<int>("DriverId").IsRequired();

            builder.HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey("TypeId");
        }
    }
}
