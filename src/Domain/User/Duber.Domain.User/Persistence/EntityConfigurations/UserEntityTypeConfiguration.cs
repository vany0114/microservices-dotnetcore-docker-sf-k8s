using Duber.Domain.User.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Duber.Domain.User.Persistence.EntityConfigurations
{
    internal class UserEntityTypeConfiguration : IEntityTypeConfiguration<Model.User>
    {
        public void Configure(EntityTypeBuilder<Model.User> builder)
        {
            builder.ToTable("Users", UserContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                .ForSqlServerUseSequenceHiLo("userseq", UserContext.DEFAULT_SCHEMA);

            builder.Ignore(b => b.DomainEvents);

            builder.Property(x => x.Name)
                .IsRequired();

            builder.Property(x => x.Email)
                .IsRequired();

            builder.Property<int>("PaymentMethodId").IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey("PaymentMethodId");

            builder.Property(x =>x.NumberPhone)
                .IsRequired(false);

            builder.Property(x => x.Rating)
                .HasDefaultValue(0)
                .IsRequired();
        }
    }
}
