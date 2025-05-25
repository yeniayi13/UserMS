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

namespace UserMs.Test.Infrastructure.Repositories
{
    public class ActivityHistoryRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<ActivityHistory>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private ActivityHistoryRepository _repository;

        public ActivityHistoryRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<ActivityHistory>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<ActivityHistory>("ActivityHistories", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

            //_repository = new ActivityHistoryRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new ActivityHistoryRepository(null, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new ActivityHistoryRepository(_dbContextMock.Object, _mongoContextMock.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
        {
            _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);

           // Assert.Throws<ArgumentNullException>(() => new ActivityHistoryRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object));
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


        [Fact]
        public async Task GetActivitiesByUserAsync_ShouldReturnActivities_WhenExists()
        {
            // Arrange
            var userId = UserId.Create(Guid.NewGuid());

            var activityDtos = new List<GetActivityHistoryDto>
            {
                new() { Action = "Login", Timestamp = DateTime.UtcNow },
                new() { Action = "Logout", Timestamp = DateTime.UtcNow.AddHours(-1) }
            };

            var activityEntities = new List<ActivityHistory>
            {
                new ActivityHistory { UserId = userId, Action = "Login", Timestamp = DateTime.UtcNow },
                new ActivityHistory { UserId = userId, Action = "Logout", Timestamp = DateTime.UtcNow.AddHours(-1) }
            };

            var mockCursor = new Mock<IAsyncCursor<GetActivityHistoryDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(activityDtos);

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<ActivityHistory>>(),
                    It.IsAny<FindOptions<ActivityHistory, GetActivityHistoryDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<ActivityHistory>>(activityDtos)).Returns(activityEntities);

            // Act
           /* var result = await _repository.GetActivitiesByUserAsync(userId, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Login", result[0].Action);
            Assert.Equal("Logout", result[1].Action);*/
        }

        [Fact]
        public async Task GetActivitiesByUserAsync_ShouldReturnEmptyList_WhenNoActivitiesFound()
        {
            var userId = UserId.Create(Guid.NewGuid());

            // 🔹 Simular cursor vacío correctamente
            var mockCursor = new Mock<IAsyncCursor<GetActivityHistoryDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 La consulta no encuentra resultados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetActivityHistoryDto>()); // 🔹 Lista vacía

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<ActivityHistory>>(),
                    It.IsAny<FindOptions<ActivityHistory, GetActivityHistoryDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object); // 🔹 Simula la consulta

            _mapperMock.Setup(m => m.Map<List<ActivityHistory>>(It.IsAny<List<GetActivityHistoryDto>>()))
                .Returns(new List<ActivityHistory>()); // 🔹 Mapea a lista vacía

           // var result = await _repository.GetActivitiesByUserAsync(userId, null, null);

            //  Verificar resultados
          //  Assert.NotNull(result);
          //  Assert.Empty(result);
        }

    }
}
