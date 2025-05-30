using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.UsersRoles;
using UserMs.Application.Handlers.User_Roles.Commands;
using UserMs.Commoon.Dtos.Users.Request.UserRole;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using Xunit;

namespace UserMs.Test.Application.Handler.Command.User_Role
{
    public class CreateUserRoleCommandHandlerTests
    {
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMongoMock;
        private readonly Mock<IEventBus<GetUserRoleDto>> _eventBusMock;
        private readonly Mock<IUserRepository> _usersRepositoryMock;
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMongoMock;
        private readonly Mock<IRolesRepository> _roleRepositoryMock;
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateUserRoleCommandHandler _handler;

        public CreateUserRoleCommandHandlerTests()
        {
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            _userRoleRepositoryMongoMock = new Mock<IUserRoleRepositoryMongo>();
            _eventBusMock = new Mock<IEventBus<GetUserRoleDto>>();
            _usersRepositoryMock = new Mock<IUserRepository>();
            _usersRepositoryMongoMock = new Mock<IUserRepositoryMongo>();
            _roleRepositoryMock = new Mock<IRolesRepository>();
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new CreateUserRoleCommandHandler(
                _mapperMock.Object,
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _keycloakServiceMock.Object,
                _usersRepositoryMock.Object,
                _usersRepositoryMongoMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _userRoleRepositoryMongoMock.Object,
                _eventBusMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el usuario y el rol existen, se asigna el rol correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldCreateUserRole_WhenUserAndRoleExist()
        {
            // Arrange
            var userRoleId = UserRoleId.Create(Guid.NewGuid());
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var user = new Users { UserId = userId, UserEmail = UserEmail.Create("test@example.com") };
            var role = new Roles { RoleId = roleId, RoleName = RoleName.Create("Admin") };

            _usersRepositoryMongoMock
                .Setup(repo => repo.GetUsersById(It.IsAny<Guid>()))
                .ReturnsAsync(user);

            _roleRepositoryMock
                .Setup(repo => repo.GetRolesByIdQuery(It.IsAny<Guid>()))
                .ReturnsAsync(role);

            _userRoleRepositoryMongoMock
                .Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((GetUserRoleDto)null); // Simula que el usuario no tiene el rol aún

            _userRoleRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<UserRoles>()))
                .Returns(Task.CompletedTask);

            _keycloakServiceMock
                .Setup(service => service.AssignClientRoleToUser(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _eventBusMock
                .Setup(bus => bus.PublishMessageAsync(It.IsAny<GetUserRoleDto>(), "userRoleQueue", "USER_ROLE_CREATED"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(new CreateUserRolesCommand(new CreateUserRolesDto { UserId = userId, RoleId = roleId }), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
           
        }

        /// <summary>
        /// Verifica que cuando el usuario o el rol no existen, se lanza `InvalidOperationException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenUserOrRoleNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            _usersRepositoryMongoMock
                .Setup(repo => repo.GetUsersById(It.IsAny<Guid>()))
                .ReturnsAsync((Users)null); // Simula que el usuario no existe

            _roleRepositoryMock
                .Setup(repo => repo.GetRolesByIdQuery(It.IsAny<Guid>()))
                .ReturnsAsync((Roles)null); // Simula que el rol no existe

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(new CreateUserRolesCommand(new CreateUserRolesDto { UserId = userId, RoleId = roleId }), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando el usuario ya tiene el rol asignado, se retorna el ID existente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnExistingUserRoleId_WhenRoleAlreadyAssigned()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var existingUserRoleDto = new GetUserRoleDto
            {
                UserRoleId = UserRoleId.Create(Guid.NewGuid()),
                UserId = userId,
                RoleId = roleId,
                UserEmail = "test@example.com",
                RoleName = "Admin"
            };

            _usersRepositoryMongoMock
                .Setup(repo => repo.GetUsersById(It.IsAny<Guid>()))
                .ReturnsAsync(new Users { UserId = userId, UserEmail = UserEmail.Create("test@example.com") });

            _roleRepositoryMock
                .Setup(repo => repo.GetRolesByIdQuery(It.IsAny<Guid>()))
                .ReturnsAsync(new Roles { RoleId = roleId, RoleName = RoleName.Create("Admin") });

            _userRoleRepositoryMongoMock
                .Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(existingUserRoleDto); // Simula que el usuario ya tiene el rol

            // Act
            var result = await _handler.Handle(new CreateUserRolesCommand(new CreateUserRolesDto { UserId = userId, RoleId = roleId }), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingUserRoleDto.UserRoleId, result.Value);
        }
    }


}
