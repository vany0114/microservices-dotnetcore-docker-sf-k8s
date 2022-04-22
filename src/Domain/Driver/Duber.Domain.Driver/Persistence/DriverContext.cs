using Duber.Domain.Driver.Model;
using Duber.Domain.Driver.Persistence.EntityConfigurations;
using Duber.Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Duber.Domain.Driver.Persistence
{
    public class DriverContext : DbContext, IUnitOfWork
    {
        // ReSharper disable once InconsistentNaming
        public const string DEFAULT_SCHEMA = "Driver";

        public DriverContext(DbContextOptions<DriverContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VehicleTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DriverStatusEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DriverEntityTypeConfiguration());
        }

        public DbSet<Model.Driver> Drivers { get; set; }

        public DbSet<DriverStatus> DriverStatuses { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<VehicleType> VehicleTypes { get; set; }
    }

    // in order to migration creation works.
    public class DriverContextDesignFactory : IDesignTimeDbContextFactory<DriverContext>
    {
        public DriverContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DriverContext>()
                .UseSqlServer("Server=.;Initial Catalog=Duber.WebSiteDb;Integrated Security=true");

            return new DriverContext(optionsBuilder.Options);
        }
    }
}
