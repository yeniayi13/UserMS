using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.ActivityHistory;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class ActivityHistoryConfiguration : IEntityTypeConfiguration<ActivityHistory>
    {
        public void Configure(EntityTypeBuilder<ActivityHistory> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.UserId).IsRequired();
            builder.Property(a => a.Action).IsRequired().HasMaxLength(255);
            builder.Property(a => a.Timestamp).IsRequired();
            builder.HasOne(a => a.User)
                .WithMany(u => u.ActivityHistories)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
