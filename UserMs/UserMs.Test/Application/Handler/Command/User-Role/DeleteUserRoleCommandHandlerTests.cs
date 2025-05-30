using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.UsersRoles;
using UserMs.Application.Handlers.User_Roles.Commands;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.User_Roles;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Command.User_Role
{
    public class DeleteUserRoleCommandHandlerTests
    {
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepository;
        private readonly Mock<IUserRoleRepositoryMongo> _mockUserRoleRepositoryMongo;
        private readonly Mock<IEventBus<GetUserRoleDto>> _mockEventBus;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IKeycloakService> _mockKeycloakMsService;
        private readonly Mock<IUserRepository> _mockUsersRepository;
        private readonly Mock<IActivityHistoryRepository> _mockActivityHistoryRepository;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _mockEventBusActivity;

        private readonly DeleteUserRoleCommandHandler _handler;

        public DeleteUserRoleCommandHandlerTests()
        {
            _mockUserRoleRepository = new Mock<IUserRoleRepository>();
            _mockUserRoleRepositoryMongo = new Mock<IUserRoleRepositoryMongo>();
            _mockEventBus = new Mock<IEventBus<GetUserRoleDto>>();
            _mockMapper = new Mock<IMapper>();
            _mockKeycloakMsService = new Mock<IKeycloakService>();
            _mockUsersRepository = new Mock<IUserRepository>();
            _mockActivityHistoryRepository = new Mock<IActivityHistoryRepository>();
            _mockEventBusActivity = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new DeleteUserRoleCommandHandler(
                _mockEventBusActivity.Object,
                _mockActivityHistoryRepository.Object,
                _mockUsersRepository.Object,
                _mockUserRoleRepositoryMongo.Object,
                _mockKeycloakMsService.Object,
                _mockUserRoleRepository.Object,
                _mockEventBus.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldDeleteUserRole_WhenUserHasRole()
        {
            // Arrange
            var userRoleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var rol = "Admin";
            var command = new DeleteUserRolesCommand(email, rol);

            var userRole = new UserRoles(userRoleId, userId, Guid.NewGuid());
            var dto = new GetUserRoleDto
            {
                UserRoleId = userRoleId,
                UserId = userId,
                UserEmail = email,
                RoleName = rol
            };

            _mockUserRoleRepositoryMongo.Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(rol, email))
                .ReturnsAsync(dto);

            _mockMapper.Setup(m => m.Map<GetUserRoleDto>(userRole))
                .Returns(new GetUserRoleDto { UserRoleId = userRoleId, UserId = userId, UserEmail = email, RoleName = rol });

            _mockUserRoleRepository.Setup(repo => repo.DeleteUsersRoleAsync(userRoleId))
                .ReturnsAsync(new UserRoles(userRoleId, Guid.NewGuid(), Guid.NewGuid())); // 🔹 Simula que la eliminación retorna el objeto eliminado
            _mockEventBus.Setup(bus => bus.PublishMessageAsync(It.IsAny<GetUserRoleDto>(), "userRoleQueue", "USER_ROLE_DELETED"))
                .Returns(Task.CompletedTask);
            _mockKeycloakMsService.Setup(service => service.RemoveClientRoleFromUser(email, rol)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(userId, result);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenUserRoleNotFound()
        {
            var command = new DeleteUserRolesCommand("user@test.com", "Admin");

            _mockUserRoleRepositoryMongo.Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(command.Rol, command.Email))
                .ReturnsAsync((GetUserRoleDto)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenEventBusFails()
        {
            var userRoleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var rol = "Admin";
            var command = new DeleteUserRolesCommand(email, rol);
            var userRole = new UserRoles(userRoleId, userId, Guid.NewGuid());
            var dto = new GetUserRoleDto
            {
                UserRoleId = userRoleId,
                UserId = userId,
                UserEmail = email,
                RoleName = rol
            };

            _mockUserRoleRepositoryMongo.Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(rol, email))
                .ReturnsAsync(dto);

            _mockMapper.Setup(m => m.Map<GetUserRoleDto>(userRole))
                .Returns(new GetUserRoleDto { UserRoleId = userRoleId, UserId = userId, UserEmail = email, RoleName = rol });

            _mockEventBus.Setup(bus => bus.PublishMessageAsync(It.IsAny<GetUserRoleDto>(), "userRoleQueue", "USER_ROLE_DELETED"))
                .ThrowsAsync(new UserNotFoundException("RabbitMQ failure"));

            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenKeycloakFails()
        {
            var userRoleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var rol = "Admin";
            var command = new DeleteUserRolesCommand(email, rol);
            var userRole = new UserRoles(userRoleId, userId, Guid.NewGuid());
            var dto = new GetUserRoleDto
            {
                UserRoleId = userRoleId,
                UserId = userId,
                UserEmail = email,
                RoleName = rol
            };
            _mockUserRoleRepositoryMongo.Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(rol, email))
                .ReturnsAsync(dto);

            _mockMapper.Setup(m => m.Map<GetUserRoleDto>(userRole))
                .Returns(new GetUserRoleDto { UserRoleId = userRoleId, UserId = userId, UserEmail = email, RoleName = rol });

            _mockKeycloakMsService.Setup(service => service.RemoveClientRoleFromUser(email, rol))
                .ThrowsAsync(new UserNotFoundException("Keycloak failure"));

            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }

}
