
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
 

    public class UsersConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasKey(s => s.UserId);
            builder.Property(s => s.UserEmail).IsRequired();
            builder.Property(s => s.UserPassword).IsRequired();
            builder.Property(s => s.UserDelete).HasDefaultValueSql("false");
            builder.Property(s => s.UserName).IsRequired();
            builder.Property(s => s.UserPhone).IsRequired();
            builder.Property(s => s.UserAddress).IsRequired();
            builder.Property(s => s.UsersType).IsRequired().HasMaxLength(50).HasConversion<string>();
            builder.Property(s => s.UserAvailable).IsRequired().HasMaxLength(50).HasConversion<string>();   

        }
    }

   
}