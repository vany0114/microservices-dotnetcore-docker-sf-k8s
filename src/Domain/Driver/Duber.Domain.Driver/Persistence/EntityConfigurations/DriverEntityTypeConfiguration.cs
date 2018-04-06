using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Duber.Domain.Driver.Persistence.EntityConfigurations
{
    internal class DriverEntityTypeConfiguration : IEntityTypeConfiguration<Model.Driver>
    {
        public void Configure(EntityTypeBuilder<Model.Driver> builder)
        {
            builder.ToTable("Drivers", DriverContext.DEFAULT_SCHEMA);

            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.DomainEvents);

            builder.Ignore(b => b.CurrentVehicle);

            builder.Property(b => b.Id)
                .ForSqlServerUseSequenceHiLo("driverseq", DriverContext.DEFAULT_SCHEMA);

            builder.Property(b => b.Name)
                .IsRequired();

            builder.Property(b => b.PhoneNumber)
                .IsRequired(false);

            builder.Property(b => b.Rating)
                .IsRequired();

            builder.Property(b => b.Email)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property<int>("StatusId").IsRequired();

            builder.HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey("StatusId");

            builder.HasMany(b => b.Vehicles)
                .WithOne()
                .HasForeignKey("DriverId")
                .OnDelete(DeleteBehavior.Cascade);

            var navigation = builder.Metadata.FindNavigation(nameof(Model.Driver.Vehicles));

            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
