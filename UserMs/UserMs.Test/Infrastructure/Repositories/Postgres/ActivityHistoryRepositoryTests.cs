using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Database;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.ActivityHistory;
using UserMs.Infrastructure.Repositories.ActivityHistoryRepo;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class ActivityHistoryRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;

        private Mock<IMapper> _mapperMock;
        private ActivityHistoryRepository _repository;

        public ActivityHistoryRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mapperMock = new Mock<IMapper>();

           

            

            _repository = new ActivityHistoryRepository(_dbContextMock.Object, _mapperMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityHistoryRepository(null,  _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityHistoryRepository(_dbContextMock.Object, null));
        }

     

        [Fact]
        public async Task AddAsync_ShouldAddActivitySuccessfully()
        {
            var activity = new ActivityHistory
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Action = "Login",
                Timestamp = DateTime.UtcNow
            };

            // Simular `EntityEntry<ActivityHistory>` para evitar errores de EF
            var mockEntityEntry = new Mock<EntityEntry<ActivityHistory>>();
            mockEntityEntry.Setup(e => e.Entity).Returns(activity);

            // Simular `AddAsync` correctamente
            _dbContextMock.Setup(db => db.ActivityHistories.AddAsync(activity, default))
                .ReturnsAsync((EntityEntry<ActivityHistory>)null); // EF no devuelve valor en pruebas

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(activity);

            // Verificar que `AddAsync` fue llamado
            _dbContextMock.Verify(db => db.ActivityHistories.AddAsync(activity, default), Times.Once);

            // Verificar que `SaveEfContextChanges` fue llamado
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }



    }
}
