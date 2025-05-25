using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Infrastructure.Database.Context.Postgress;
using UserMs.Infrastructure.Database.Factory.Postgres;
using Xunit;

namespace UserMs.Test.Infrastructure.DataBase
{
    public class ApplicationDbContextFactoryTests
    {
        [Fact]
        public void CreateDbContext_ShouldReturnValidDbContext()
        {
            // Arrange
            var factory = new UserDbContextFactory();

            // Act
            var dbContext = factory.CreateDbContext(new string[] { });

            // Assert
            Assert.NotNull(dbContext);
            Assert.IsType<UserDbContext>(dbContext);
        }

        [Fact]
        public void CreateDbContext_ShouldUsePostgreSQLConfiguration()
        {
            // Arrange
            var factory = new UserDbContextFactory();

            // Act
            var dbContext = factory.CreateDbContext(new string[] { });

            // Assert
            Assert.Contains("Npgsql", dbContext.Database.ProviderName); // 🔥 Confirma que está usando PostgreSQL
        }
    }
}
