using Duber.WebSite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Duber.WebSite.Infrastructure.Persistence
{
    public class ReportingContext : DbContext
    {
        // ReSharper disable once InconsistentNaming
        private const string DEFAULT_SCHEMA = "Reporting";

        public ReportingContext(DbContextOptions<ReportingContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DEFAULT_SCHEMA);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Trip> Trips { get; set; }
    }

    public class ReportingContextDesignFactory : IDesignTimeDbContextFactory<ReportingContext>
    {
        public ReportingContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ReportingContext>()
                .UseSqlServer("Server=.;Initial Catalog=Duber.WebSiteDb;Integrated Security=true");

            return new ReportingContext(optionsBuilder.Options);
        }
    }
}
