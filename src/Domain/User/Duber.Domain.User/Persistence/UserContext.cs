using Duber.Domain.SharedKernel.Model;
using Duber.Domain.User.Persistence.EntityConfigurations;
using Duber.Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Duber.Domain.User.Persistence
{
    public class UserContext : DbContext, IUnitOfWork
    {
        // ReSharper disable once InconsistentNaming
        public const string DEFAULT_SCHEMA = "User";

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentMethodEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        }

        public DbSet<Model.User> Users { get; set; }

        public DbSet<PaymentMethod> PaymentMethods { get; set; }
    }

    // in order to migration creation works.
    public class UserContextDesignFactory : IDesignTimeDbContextFactory<UserContext>
    {
        public UserContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserContext>()
                .UseSqlServer("Server=.;Initial Catalog=Duber.WebSiteDb;Integrated Security=true");

            return new UserContext(optionsBuilder.Options);
        }
    }
}
