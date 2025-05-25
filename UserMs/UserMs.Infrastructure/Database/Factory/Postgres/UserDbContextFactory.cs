using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using UserMs.Infrastructure.Database.Context.Postgress;


namespace UserMs.Infrastructure.Database.Factory.Postgres
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=UserMs;Username=postgres;Password=yeniree0813");

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
