using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Handlers.Bidder.Command;
using UserMs.Application.Validators;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Exceptions;
using Xunit;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Handlers.Bidder.Command;

namespace UserMs.Test.Application.Handler.Command.Bidder
{
    public class UpdateBidderCommandHandlerTests
    {
        private readonly Mock<IBidderRepository> _mockBidderRepository;
        private readonly Mock<IBidderRepositoryMongo> _mockBidderRepositoryMongo;
        private readonly Mock<IEventBus<GetBidderDto>> _mockEventBus;
        private readonly Mock<IUserRepository> _mockUsersRepository;
        private readonly Mock<IUserRepositoryMongo> _mockUsersRepositoryMongo;
        private readonly Mock<IEventBus<GetUsersDto>> _mockEventBusUser;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IKeycloakService> _mockKeycloakRepository;
        private readonly Mock<IActivityHistoryRepository> _mockActivityHistoryRepository;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _mockEventBusActivity;

        private readonly UpdateBidderCommandHandler _handler;

        public UpdateBidderCommandHandlerTests()
        {
            _mockBidderRepository = new Mock<IBidderRepository>();
            _mockBidderRepositoryMongo = new Mock<IBidderRepositoryMongo>();
            _mockEventBus = new Mock<IEventBus<GetBidderDto>>();
            _mockUsersRepository = new Mock<IUserRepository>();
            _mockUsersRepositoryMongo = new Mock<IUserRepositoryMongo>();
            _mockEventBusUser = new Mock<IEventBus<GetUsersDto>>();
            _mockMapper = new Mock<IMapper>();
            _mockKeycloakRepository = new Mock<IKeycloakService>();
            _mockActivityHistoryRepository = new Mock<IActivityHistoryRepository>();
            _mockEventBusActivity = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new UpdateBidderCommandHandler(
                _mockBidderRepository.Object,
                _mockBidderRepositoryMongo.Object,
                _mockEventBus.Object,
                _mockUsersRepository.Object,
                _mockUsersRepositoryMongo.Object,
                _mockEventBusUser.Object,
                _mockMapper.Object,
                _mockKeycloakRepository.Object,
                _mockActivityHistoryRepository.Object,
                _mockEventBusActivity.Object
            );
        }

       /* [Fact]
        public async Task Handle_ShouldUpdateBidder_WhenValidRequest()
        {
            // Arrange
            var bidderId = UserId.Create();

            var updateCommand = new UpdateBidderCommand(bidderId, new UpdateBidderDto
            {
                UserEmail = "test@example.com",
                UserName = "Carlos Pérez",
                UserPhone = "04123456789",
                UserAddress = "Avenida Principal, Caracas, Venezuela",
                UserLastName = "Pérez",
                BidderDni = "12345678",
                BidderBirthday = new DateOnly(1990, 5, 20),
                BidderDelete = false
            });

            // 🔹 Crear postor existente con datos válidos
            var existingBidder = new Bidders
            {
                UserName = UserName.Create("Carlos Pérez"),
                UserPhone = UserPhone.Create("04123456789"),
                UserAddress = UserAddress.Create("Avenida Principal, Caracas, Venezuela"),
                UserLastName = UserLastName.Create("Pérez"),
                BidderDni = BidderDni.Create("12345678"),
                BidderBirthday = BidderBirthday.Create(new DateOnly(1990, 5, 20)),
                BidderDelete = BidderDelete.Create(false)
            };

            // 🔹 Crear usuario existente con datos válidos
            var existingUser = new Users(
                UserId.Create(bidderId.Value),
                UserEmail.Create("test@example.com"),
                UserPassword.Create("SecurePass123"),
                UserName.Create("Carlos Pérez"),
                UserPhone.Create("04123456789"),
                UserAddress.Create("Avenida Principal, Caracas, Venezuela"),
                UserLastName.Create("Pérez"),
                Enum.Parse<UsersType>("Postor"),
                Enum.Parse<UserAvailable>("Activo"),
                UserDelete.Create(false)
            );

            _mockBidderRepositoryMongo.Setup(r => r.GetBidderByIdAsync(bidderId.Value))
                .ReturnsAsync(existingBidder ?? new Bidders());
            _mockUsersRepositoryMongo.Setup(r => r.GetUsersById(bidderId.Value))
                .ReturnsAsync(existingUser ?? new Users(UserId.Create(bidderId.Value)));

            _mockBidderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<UserId>(), It.IsAny<Bidders>()))
                .ReturnsAsync(new Bidders());
            _mockUsersRepository.Setup(repo => repo.UpdateUsersAsync(It.IsAny<UserId>(), It.IsAny<Users>()))
                .ReturnsAsync(new Users());

            _mockEventBus.Setup(e => e.PublishMessageAsync(It.IsAny<GetBidderDto>(), "bidderQueue", "BIDDER_UPDATED"))
                .Returns(Task.CompletedTask);

            _mockEventBusUser.Setup(e => e.PublishMessageAsync(It.IsAny<GetUsersDto>(), "userQueue", "USER_UPDATED"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(updateCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
       */
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenBidderDoesNotExist()
        {
            var updateCommand = new UpdateBidderCommand(UserId.Create(), new UpdateBidderDto());

            _mockBidderRepositoryMongo.Setup(r => r.GetBidderByIdAsync(It.IsAny<UserId>())).ReturnsAsync((Bidders)null);

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(updateCommand, CancellationToken.None));
        }

        

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDatabaseUpdateFails()
        {
            var bidderId = UserId.Create();
            var updateCommand = new UpdateBidderCommand(bidderId, new UpdateBidderDto { UserEmail = "test@example.com" });

            var existingBidder = new Bidders();
            var existingUser = new Users(UserId.Create(bidderId.Value));

            _mockBidderRepositoryMongo.Setup(r => r.GetBidderByIdAsync(bidderId)).ReturnsAsync(existingBidder);
            _mockUsersRepositoryMongo.Setup(r => r.GetUsersById(bidderId.Value)).ReturnsAsync(existingUser);

            _mockBidderRepository.Setup(r => r.UpdateAsync(bidderId, existingBidder)).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(updateCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenEventBusFails()
        {
            var bidderId = UserId.Create();
            var updateCommand = new UpdateBidderCommand(bidderId, new UpdateBidderDto { UserEmail = "test@example.com" });

            var existingBidder = new Bidders();
            var existingUser = new Users(UserId.Create(bidderId.Value));

            _mockBidderRepositoryMongo.Setup(r => r.GetBidderByIdAsync(bidderId)).ReturnsAsync(existingBidder);
            _mockUsersRepositoryMongo.Setup(r => r.GetUsersById(bidderId.Value)).ReturnsAsync(existingUser);

            _mockEventBus.Setup(e => e.PublishMessageAsync(It.IsAny<GetBidderDto>(), "bidderQueue", "BIDDER_UPDATED"))
                .ThrowsAsync(new Exception("RabbitMQ error"));

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(updateCommand, CancellationToken.None));
        }
    }

}
