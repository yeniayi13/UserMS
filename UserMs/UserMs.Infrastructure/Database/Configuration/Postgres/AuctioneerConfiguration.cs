using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class AuctioneersConfiguration : IEntityTypeConfiguration<Auctioneers>
    {
        public void Configure(EntityTypeBuilder<Auctioneers> builder)
        {
            builder.HasKey(s => s.UserId);
            builder.Property(s => s.UserEmail).IsRequired();
            builder.Property(s => s.UserPassword).IsRequired();
            builder.Property(s => s.UserName).IsRequired();
            builder.Property(s => s.UserPhone).IsRequired();
            builder.Property(s => s.UserAddress).IsRequired();
            builder.Property(s => s.UserLastName).IsRequired();
            builder.Property(a => a.AuctioneerDni).IsRequired();
            builder.Property(a => a.AuctioneerBirthday).IsRequired();
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.CreatedBy).HasMaxLength(255);
            builder.Property(a => a.UpdatedAt);
            builder.Property(a => a.UpdatedBy).HasMaxLength(255);
            builder.Property(a => a.AuctioneerDelete).HasDefaultValueSql("false");

            // Relación con Users
           
        }
    }
}
