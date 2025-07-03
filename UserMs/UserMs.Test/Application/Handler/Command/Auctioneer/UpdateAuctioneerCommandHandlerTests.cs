using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Application.Handlers.Auctioneer.Command;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Infrastructure.Exceptions;
using Xunit;
using Handlers.Auctioneer.Command;

namespace UserMs.Test.Application.Handler.Command.Auctioneer
{
    public class UpdateAuctioneerCommandHandlerTests
    {
        private readonly Mock<IAuctioneerRepository> _mockAuctioneerRepository;
        private readonly Mock<IAuctioneerRepositoryMongo> _mockAuctioneerRepositoryMongo;
        private readonly Mock<IEventBus<GetAuctioneerDto>> _mockEventBus;
        private readonly Mock<IUserRepository> _mockUsersRepository;
        private readonly Mock<IUserRepositoryMongo> _mockUsersRepositoryMongo;
        private readonly Mock<IEventBus<GetUsersDto>> _mockEventBusUser;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IKeycloakService> _mockKeycloakRepository;
        private readonly Mock<IActivityHistoryRepository> _mockActivityHistoryRepository;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _mockEventBusActivity;

        private readonly UpdateAuctioneerCommandHandler _handler;

        public UpdateAuctioneerCommandHandlerTests()
        {
            _mockAuctioneerRepository = new Mock<IAuctioneerRepository>();
            _mockAuctioneerRepositoryMongo = new Mock<IAuctioneerRepositoryMongo>();
            _mockEventBus = new Mock<IEventBus<GetAuctioneerDto>>();
            _mockUsersRepository = new Mock<IUserRepository>();
            _mockUsersRepositoryMongo = new Mock<IUserRepositoryMongo>();
            _mockEventBusUser = new Mock<IEventBus<GetUsersDto>>();
            _mockMapper = new Mock<IMapper>();
            _mockKeycloakRepository = new Mock<IKeycloakService>();
            _mockActivityHistoryRepository = new Mock<IActivityHistoryRepository>();
            _mockEventBusActivity = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new UpdateAuctioneerCommandHandler(
                _mockEventBusActivity.Object,
                _mockActivityHistoryRepository.Object,
                _mockAuctioneerRepository.Object,
                _mockAuctioneerRepositoryMongo.Object,
                _mockEventBus.Object,
                _mockKeycloakRepository.Object,
                _mockMapper.Object,
                _mockEventBusUser.Object,
                _mockUsersRepository.Object,
                _mockUsersRepositoryMongo.Object
            );

        }

        /* [Fact]
         public async Task Handle_ShouldUpdateAuctioneer_WhenValidRequest()
         {
             // Arrange
             var auctioneerId = UserId.Create();
             var updateCommand = new UpdateAuctioneerCommand(auctioneerId, new UpdateAuctioneerDto
             {
                 UserEmail = "carlos.perez@example.com",
                 UserName = "Carlos Pérez",
                 UserPhone = "+58-412-3456789",
                 UserAddress = "Avenida Principal, Caracas, Venezuela",
                 UserLastName = "Pérez",
                 AuctioneerDni = "V-12345678",
                 AuctioneerBirthday = new DateTime(1990, 5, 20)
             });

             var existingAuctioneer = new Auctioneers
             {
                 UserName = UserName.Create("Carlos Pérez"),
                 UserPhone = UserPhone.Create("+58-412-3456789"),
                 UserAddress = UserAddress.Create("Avenida Principal, Caracas, Venezuela"),
                 UserLastName = UserLastName.Create("Pérez"),
                 AuctioneerDni = AuctioneerDni.Create("V-12345678"),
                 AuctioneerBirthday = AuctioneerBirthday.Create(new DateTime(1990, 5, 20))
             };

             var existingUser = new Users(UserId.Create(auctioneerId.Value), UserEmail.Create("carlos.perez@example.com"));

             _mockAuctioneerRepositoryMongo.Setup(r => r.GetAuctioneerByIdAsync(auctioneerId)).ReturnsAsync(existingAuctioneer);
             _mockUsersRepositoryMongo.Setup(r => r.GetUsersById(auctioneerId.Value)).ReturnsAsync(existingUser);

             _mockAuctioneerRepository.Setup(r => r.UpdateAsync(auctioneerId, existingAuctioneer)).Returns(Task.CompletedTask);
             _mockUsersRepository.Setup(r => r.UpdateUsersAsync(auctioneerId, existingUser)).Returns(Task.CompletedTask);

             _mockEventBus.Setup(e => e.PublishMessageAsync(It.IsAny<GetAuctioneerDto>(), "auctioneerQueue", "AUCTIONEER_UPDATED"))
                 .Returns(Task.CompletedTask);

             _mockEventBusUser.Setup(e => e.PublishMessageAsync(It.IsAny<GetUsersDto>(), "userQueue", "USER_UPDATED"))
                 .Returns(Task.CompletedTask);

             // Act
             var result = await _handler.Handle(updateCommand, CancellationToken.None);

             // Assert
             Assert.NotNull(result);
         }*/

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenAuctioneerDoesNotExist()
        {
            var updateCommand = new UpdateAuctioneerCommand(UserId.Create(), new UpdateAuctioneerDto());

            _mockAuctioneerRepositoryMongo.Setup(r => r.GetAuctioneerByIdAsync(It.IsAny<UserId>())).ReturnsAsync((Auctioneers)null);

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(updateCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDatabaseUpdateFails()
        {
            var auctioneerId = UserId.Create();
            var updateCommand = new UpdateAuctioneerCommand(auctioneerId, new UpdateAuctioneerDto { UserEmail = "carlos.perez@example.com" });

            var existingAuctioneer = new Auctioneers();
            var existingUser = new Users(UserId.Create(auctioneerId.Value));

            _mockAuctioneerRepositoryMongo.Setup(r => r.GetAuctioneerByIdAsync(auctioneerId)).ReturnsAsync(existingAuctioneer);
            _mockUsersRepositoryMongo.Setup(r => r.GetUsersById(auctioneerId.Value)).ReturnsAsync(existingUser);

            _mockAuctioneerRepository.Setup(r => r.UpdateAsync(auctioneerId, existingAuctioneer)).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(updateCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenEventBusFails()
        {
            var auctioneerId = UserId.Create();
            var updateCommand = new UpdateAuctioneerCommand(auctioneerId, new UpdateAuctioneerDto { UserEmail = "carlos.perez@example.com" });

            var existingAuctioneer = new Auctioneers();
            var existingUser = new Users(UserId.Create(auctioneerId.Value));

            _mockAuctioneerRepositoryMongo.Setup(r => r.GetAuctioneerByIdAsync(auctioneerId)).ReturnsAsync(existingAuctioneer);
            _mockUsersRepositoryMongo.Setup(r => r.GetUsersById(auctioneerId.Value)).ReturnsAsync(existingUser);

            _mockEventBus.Setup(e => e.PublishMessageAsync(It.IsAny<GetAuctioneerDto>(), "auctioneerQueue", "AUCTIONEER_UPDATED"))
                .ThrowsAsync(new Exception("RabbitMQ error"));

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(updateCommand, CancellationToken.None));
        }
    }

}
