using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Bidder;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class BiddersConfiguration : IEntityTypeConfiguration<Bidders>
    {
        public void Configure(EntityTypeBuilder<Bidders> builder)
        {
            builder.HasKey(s => s.UserId);
            builder.Property(s => s.UserEmail).IsRequired();
            builder.Property(s => s.UserPassword).IsRequired();
            builder.Property(s => s.UserName).IsRequired();
            builder.Property(s => s.UserPhone).IsRequired();
            builder.Property(s => s.UserAddress).IsRequired();
            builder.Property(s => s.UserLastName).IsRequired();
            builder.Property(b => b.BidderDni).IsRequired();
            builder.Property(b => b.BidderBirthday).IsRequired();
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.CreatedBy).HasMaxLength(255);
            builder.Property(b => b.UpdatedAt);
            builder.Property(b => b.UpdatedBy).HasMaxLength(255);
            builder.Property(b => b.BidderDelete).HasDefaultValueSql("false");

        }
    }
}
