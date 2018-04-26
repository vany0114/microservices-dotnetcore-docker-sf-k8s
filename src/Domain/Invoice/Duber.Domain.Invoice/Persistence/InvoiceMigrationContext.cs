using System;
using Duber.Domain.SharedKernel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Duber.Domain.Invoice.Persistence
{
    /// <summary>
    /// This context is only to creates and runs the migrations. Just to example purposes and, avoids that you have to deal running scripts before to execute the solution.
    /// You must use <see cref="IInvoiceContext"/>
    /// </summary>
    [Obsolete("This context is only to creates and runs the migrations. Just to example purposes and, avoids that you have to deal running scripts before to execute the solution.")]
    public class InvoiceMigrationContext : DbContext
    {
        public InvoiceMigrationContext(DbContextOptions<InvoiceMigrationContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentInfoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceEntityTypeConfiguration());
        }

        public DbSet<Model.Invoice> Invoices { get; set; }

        public DbSet<PaymentInfo> PaymentsInfo { get; set; }
    }

    public class UserContextDesignFactory : IDesignTimeDbContextFactory<InvoiceMigrationContext>
    {
        public InvoiceMigrationContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InvoiceMigrationContext>()
                .UseSqlServer("Server=.;Initial Catalog=Duber.InvoiceDb;Integrated Security=true");

            return new InvoiceMigrationContext(optionsBuilder.Options);
        }
    }

    internal class InvoiceEntityTypeConfiguration : IEntityTypeConfiguration<Model.Invoice>
    {
        public void Configure(EntityTypeBuilder<Model.Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(o => o.InvoiceId);
            
            builder.Ignore(b => b.DomainEvents);

            builder.Ignore(b => b.Id);

            builder.Property(x => x.Fee)
                .IsRequired();

            builder.Property(x => x.Total)
                .IsRequired();

            builder.Property(x => x.Created)
                .IsRequired();

            builder.Property<int>("PaymentMethodId").IsRequired();

            builder.Property<int>("TripStatusId").IsRequired();

            builder.Property<Guid>("TripId").IsRequired();

            builder.Property<double>("Distance").IsRequired();

            builder.Property<TimeSpan>("Duration").IsRequired();

            builder.HasOne(a => a.PaymentInfo)
                .WithOne()
                .HasForeignKey<PaymentInfo>(b => b.InvoiceId);
        }
    }

    internal class PaymentInfoEntityTypeConfiguration : IEntityTypeConfiguration<PaymentInfo>
    {
        public void Configure(EntityTypeBuilder<PaymentInfo> builder)
        {
            builder.ToTable("PaymentsInfo");

            builder.HasKey(o => new { o.Status, o.CardNumber, o.CardType, o.InvoiceId, o.UserId });

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CardNumber)
                .IsRequired();

            builder.Property(x => x.CardType)
                .IsRequired();

            builder.Property(x => x.InvoiceId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();
        }
    }
}
