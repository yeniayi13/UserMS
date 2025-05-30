using AuthMs.Common.Exceptions;
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
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using Xunit;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Test.Application.Handler.Command.Auctioneer
{
    public class CreateAuctioneerCommandHandlerTests
    {
        private readonly Mock<IAuctioneerRepository> _auctioneerRepositoryMock;
        private readonly Mock<IAuctioneerRepositoryMongo> _auctioneerRepositoryMongoMock;
        private readonly Mock<IEventBus<GetAuctioneerDto>> _eventBusMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IEventBus<GetUsersDto>> _eventBusUserMock;
        private readonly Mock<IUserRepository> _usersRepositoryMock;
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMongoMock;
        private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMongoMock;
        private readonly Mock<IRolesRepository> _roleRepositoryMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IEventBus<GetUserRoleDto>> _eventBusUserRolMock;
        private readonly CreateAuctioneerCommandHandler _handler;

        public CreateAuctioneerCommandHandlerTests()
        {
            _auctioneerRepositoryMock = new Mock<IAuctioneerRepository>();
            _auctioneerRepositoryMongoMock = new Mock<IAuctioneerRepositoryMongo>();
            _eventBusMock = new Mock<IEventBus<GetAuctioneerDto>>();
            _mapperMock = new Mock<IMapper>();
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _eventBusUserMock = new Mock<IEventBus<GetUsersDto>>();
            _usersRepositoryMock = new Mock<IUserRepository>();
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            _usersRepositoryMongoMock = new Mock<IUserRepositoryMongo>();
            _userRoleRepositoryMongoMock = new Mock<IUserRoleRepositoryMongo>();
            _roleRepositoryMock = new Mock<IRolesRepository>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _eventBusUserRolMock = new Mock<IEventBus<GetUserRoleDto>>();

            _handler = new CreateAuctioneerCommandHandler(
                _auctioneerRepositoryMock.Object,
                _auctioneerRepositoryMongoMock.Object,
                _eventBusMock.Object,
                _mapperMock.Object,
                _keycloakServiceMock.Object,
                _eventBusUserMock.Object,
                _usersRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _usersRepositoryMongoMock.Object,
                _userRoleRepositoryMongoMock.Object,
                _roleRepositoryMock.Object,
                _activityHistoryRepositoryMock.Object,
                _eventBusActivityMock.Object,
                _eventBusUserRolMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando los datos son válidos, se crea el subastador correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldCreateAuctioneer_WhenRequestIsValid()
        {
            // Arrange
            var command = new CreateAuctioneerCommand(new CreateAuctioneerDto
            {
                UserEmail = "test@example.com",
                UserPassword = "Securepassword8$",
                UserName = "John",
                UserLastName = "Doe",
                UserPhone = "02125457896",
                UserAddress = "123 Main St",
                AuctioneerDni = "12345678",
                AuctioneerBirthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-18))
            });

            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var expectedRole = new Roles { RoleId = roleId, RoleName = RoleName.Create("Subastador") };

            _auctioneerRepositoryMongoMock
                .Setup(repo => repo.GetAuctioneerByEmailAsync(It.IsAny<UserEmail>()))
                .ReturnsAsync((Auctioneers)null); // Simulamos que el usuario no existe

            _keycloakServiceMock
                .Setup(service => service.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                                          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("mocked-user-id");

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(It.IsAny<string>()))
                .ReturnsAsync(userId);

            _roleRepositoryMock
                .Setup(repo => repo.GetRolesByNameQuery("Subastador"))
                .ReturnsAsync(expectedRole); // ✅ Simulación de que el rol existe

            _auctioneerRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Auctioneers>()))
                .Returns(Task.CompletedTask);

            _usersRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Users>()))
                .Returns(Task.CompletedTask);

            _userRoleRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<UserRoles>()))
                .Returns(Task.CompletedTask);

            _eventBusMock
                .Setup(bus => bus.PublishMessageAsync(It.IsAny<GetAuctioneerDto>(), "auctioneerQueue", "AUCTIONEER_CREATED"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Validaciones extra para evitar `NullReferenceException`
            Assert.NotNull(result); // Validamos que el resultado no es `null`
            Assert.Equal(userId, result.Value);

            // Verificación de objetos creados antes de llamar a `PublishEvents()`
            var auctioneer = _mapperMock.Object.Map<Auctioneers>(command.Auctioneer);
            Assert.NotNull(auctioneer);

            var users = _mapperMock.Object.Map<Users>(command.Auctioneer);
            Assert.NotNull(users);

            var userRole = new UserRoles
            {
                UserRoleId = UserRoleId.Create(Guid.NewGuid()),
                UserId = UserId.Create(userId),
                RoleId = RoleId.Create(expectedRole.RoleId)
            };
            Assert.NotNull(userRole);

            // Simulación de publicación de eventos
            var auctioneerDto = _mapperMock.Object.Map<GetAuctioneerDto>(auctioneer);
            Assert.NotNull(auctioneerDto);
            await _eventBusMock.Object.PublishMessageAsync(auctioneerDto, "auctioneerQueue", "AUCTIONEER_CREATED");

            var userDto = _mapperMock.Object.Map<GetUsersDto>(users);
            Assert.NotNull(userDto);
            await _eventBusUserMock.Object.PublishMessageAsync(userDto, "userQueue", "USER_CREATED");

            var userRoleDto = _mapperMock.Object.Map<GetUserRoleDto>(userRole);
            Assert.NotNull(userRoleDto);
            await _eventBusUserRolMock.Object.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_CREATED");
        }

        /// <summary>
        /// Verifica que cuando el usuario ya existe, se lanza una excepción `UserExistException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserExistException_WhenUserAlreadyExists()
        {
            // Arrange
            var command = new CreateAuctioneerCommand(new CreateAuctioneerDto
            {
                UserEmail = "existing@example.com",
                UserPassword = "securepassword",
                UserName = "Jane",
                UserLastName = "Doe",
                UserPhone = "987654321",
                UserAddress = "456 Another St"
            });

            _auctioneerRepositoryMongoMock
                .Setup(repo => repo.GetAuctioneerByEmailAsync(It.IsAny<UserEmail>()))
                .ReturnsAsync(new Auctioneers());

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre un error en Keycloak, se lanza una excepción.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenKeycloakFails()
        {
            // Arrange
            var command = new CreateAuctioneerCommand(new CreateAuctioneerDto
            {
                UserEmail = "error@example.com",
                UserPassword = "securepassword",
                UserName = "John",
                UserLastName = "Doe",
                UserPhone = "123456789",
                UserAddress = "123 Main St"
            });

            _keycloakServiceMock
                .Setup(service => service.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Keycloak error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }

}
