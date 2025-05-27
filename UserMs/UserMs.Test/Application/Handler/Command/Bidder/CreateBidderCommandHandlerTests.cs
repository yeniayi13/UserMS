using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthMs.Common.Exceptions;
using FluentValidation;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Handlers.Bidder.Command;
using UserMs.Application.Validators;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Role;
using Xunit;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.User_Roles;

namespace UserMs.Test.Application.Handler.Command.Bidder
{
    public class CreateBidderCommandHandlerTests
    {
        /*  private readonly Mock<IBidderRepository> _bidderRepositoryMock = new();
          private readonly Mock<IBidderRepositoryMongo> _bidderRepositoryMongoMock = new();
          private readonly Mock<IEventBus<GetBidderDto>> _eventBusMock = new();
          private readonly Mock<IMapper> _mapperMock = new();
          private readonly Mock<IKeycloakService> _keycloakMsServiceMock = new();
          private readonly Mock<IUserRepository> _usersRepositoryMock = new();
          private readonly Mock<IEventBus<GetUsersDto>> _eventBusUserMock = new();
          private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock = new();
          private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMongoMock = new();
          private readonly Mock<IRolesRepository> _roleRepositoryMock = new();
          private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock = new();
          private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock = new();
          private readonly Mock<IEventBus<GetUserRoleDto>> _eventBusUserRolMock = new();

          private readonly CreateBidderCommandHandler _handler;

          public CreateBidderCommandHandlerTests()
          {
              _handler = new CreateBidderCommandHandler(
                  _eventBusUserRolMock.Object,
                  _eventBusActivityMock.Object,
                  _activityHistoryRepositoryMock.Object,
                  _userRoleRepositoryMock.Object,
                  _roleRepositoryMock.Object,
                  _usersRepositoryMock.Object,
                  _userRoleRepositoryMongoMock.Object,
                  _bidderRepositoryMock.Object,
                  _bidderRepositoryMongoMock.Object,
                  _eventBusMock.Object,
                  _mapperMock.Object,
                  _keycloakMsServiceMock.Object,
                  _eventBusUserMock.Object
              );
          }

          [Fact]
          public async Task Handle_ShouldCreateBidder_WhenValidRequest()
          {
              //  Arrange
              var bidderDto = new CreateBidderDto
              {
                  UserEmail = "test@example.com",
                  UserPassword = "Test@1234", // 🔹 Contraseña válida con carácter especial
                  UserName = "Test User",
                  UserLastName = "Lastname",
                  UserPhone = "1234567890", // 🔹 Teléfono válido con 10 dígitos
                  UserAddress = "Test Address",
                  BidderDni = "12345678",
                  BidderBirthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-18)) // 🔹 Usuario mayor de edad
              };

              var request = new CreateBidderCommand(bidderDto);

              // 🔹 Simular que el usuario no existe en MongoDB
              _bidderRepositoryMongoMock.Setup(repo => repo.GetBidderByEmailAsync(It.IsAny<UserEmail>()))
                  .ReturnsAsync((Bidders)null);

              // 🔹 Simular creación de usuario en Keycloak
              _keycloakMsServiceMock.Setup(service => service.CreateUserAsync(
                  request.Bidder.UserEmail, request.Bidder.UserPassword, request.Bidder.UserName,
                  request.Bidder.UserLastName, request.Bidder.UserPhone, request.Bidder.UserAddress))
                  .ReturnsAsync("mocked-user-id");

              // 🔹 Simular recuperación de usuario desde Keycloak
              var mockedUserId = Guid.NewGuid();
              _keycloakMsServiceMock.Setup(service => service.GetUserByUserName(request.Bidder.UserEmail))
                  .ReturnsAsync(mockedUserId);

              // 🔹 Simular rol "Postor" existe en la base de datos
              _roleRepositoryMock.Setup(repo => repo.GetRolesByNameQuery("Postor"))
                  .ReturnsAsync(new Roles
                  (
                      RoleId.Create(Guid.NewGuid()),
                      RoleName.Create("Postor")
                  ));

              // 🔹 Simular que el usuario no tiene el rol asignado
              _userRoleRepositoryMongoMock.Setup(repo => repo.GetRoleByIdAndByUserIdQuery("Postor", request.Bidder.UserEmail))
                  .ReturnsAsync((GetUserRoleDto)null);

              // 🔹 Simular publicación de eventos correctamente
              _eventBusMock.Setup(bus => bus.PublishMessageAsync(It.IsAny<GetBidderDto>(), "bidderQueue", "BIDDER_CREATED"))
                  .Returns(Task.CompletedTask);

              _eventBusUserMock.Setup(bus => bus.PublishMessageAsync(It.IsAny<GetUsersDto>(), "userQueue", "USER_CREATED"))
                  .Returns(Task.CompletedTask);

              _eventBusUserRolMock.Setup(bus => bus.PublishMessageAsync(It.IsAny<GetUserRoleDto>(), "userRoleQueue", "USER_ROLE_CREATED"))
                  .Returns(Task.CompletedTask);

              _eventBusActivityMock.Setup(bus => bus.PublishMessageAsync(It.IsAny<GetActivityHistoryDto>(), "activityHistoryQueue", "ACTIVITY_CREATED"))
                  .Returns(Task.CompletedTask);

              // ✅ Act
              var result = await _handler.Handle(request, CancellationToken.None);

              // ✅ Assert
              Assert.NotNull(result); // 🔹 El resultado no debe ser nulo
              _bidderRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Bidders>()), Times.Once); // 🔹 Se guardó en MongoDB
              _eventBusMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetBidderDto>(), "bidderQueue", "BIDDER_CREATED"), Times.Once); // 🔹 Se publicó evento de creación
              _eventBusUserMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetUsersDto>(), "userQueue", "USER_CREATED"), Times.Once); // 🔹 Se publicó evento de usuario
              _eventBusUserRolMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetUserRoleDto>(), "userRoleQueue", "USER_ROLE_CREATED"), Times.Once); // 🔹 Se publicó evento de rol
              _eventBusActivityMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetActivityHistoryDto>(), "activityHistoryQueue", "ACTIVITY_CREATED"), Times.Once); // 🔹 Se registró actividad
          }

          [Fact]
          public async Task Handle_ShouldThrowValidationException_WhenBidderDataIsInvalid()
          {
              // Arrange
              var bidderDto = new CreateBidderDto
              {

              };

              var request = new CreateBidderCommand(bidderDto);


              var validatorMock = new Mock<CreateBidderValidator>();
              validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateBidderDto>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new FluentValidation.Results.ValidationResult(new List<FluentValidation.Results.ValidationFailure>
                  {
                      new FluentValidation.Results.ValidationFailure("UserEmail", "Email is required")
                  }));

              // Act & Assert
              await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
          }

          [Fact]
          public async Task Handle_ShouldThrowUserExistException_WhenBidderAlreadyExists()
          {
              // Arrange
              var bidderDto = new CreateBidderDto
              {
                  UserEmail = "test@example.com",
                  UserPassword = "Test@1234",
                  UserName = "Test User",
                  UserLastName = "Lastname",
                  UserPhone = "1234567890",
                  UserAddress = "Test Address",
                  BidderDni = "12345678",
                  BidderBirthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-18))
              };

              var request = new CreateBidderCommand(bidderDto);

              _bidderRepositoryMongoMock.Setup(repo => repo.GetBidderByEmailAsync(It.IsAny<UserEmail>()))
                  .ReturnsAsync(new Bidders(UserId.Create(Guid.NewGuid()), UserEmail.Create(request.Bidder.UserEmail),
                      UserPassword.Create("password"), UserName.Create("Test Name"),
                      UserPhone.Create("12345"), UserAddress.Create("Test Address"),
                      UserLastName.Create("Lastname"), BidderDni.Create("123456"),
                      BidderBirthday.Create(DateOnly.FromDateTime(DateTime.Now))));

              // Act & Assert
              await Assert.ThrowsAsync<UserExistException>(() => _handler.Handle(request, CancellationToken.None));
          }
          [Fact]
          public async Task Handle_ShouldThrowException_WhenKeycloakFails()
          {
              // Arrange
              var bidderDto = new CreateBidderDto
              {
                  UserEmail = "test@example.com",
                  UserPassword = "Test@1234",
                  UserName = "Test User",
                  UserLastName = "Lastname",
                  UserPhone = "1234567890",
                  UserAddress = "Test Address",
                  BidderDni = "12345678",
                  BidderBirthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-18)) // 🔹 Usuario debe ser mayor de edad
              };

              var request = new CreateBidderCommand(bidderDto);

              _keycloakMsServiceMock.Setup(service => service.CreateUserAsync(
                      It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                  .ThrowsAsync(new Exception("Keycloak error"));

              // Act & Assert
              await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
          }

          [Fact]
          public async Task Handle_ShouldThrowException_WhenRoleAssignmentFails()
          {
              // Arrange
              // Arrange
              var bidderDto = new CreateBidderDto
              {
                  UserEmail = "test@example.com",
                  UserPassword = "Test@1234",
                  UserName = "Test User",
                  UserLastName = "Lastname",
                  UserPhone = "1234567890",
                  UserAddress = "Test Address",
                  BidderDni = "12345678",
                  BidderBirthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-18))
              };
              var request = new CreateBidderCommand(bidderDto);

              _keycloakMsServiceMock.Setup(service => service.CreateUserAsync(
                  It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

              _keycloakMsServiceMock.Setup(service => service.AssignClientRoleToUser(It.IsAny<Guid>(), "Postor"))
                  .ThrowsAsync(new Exception("Role assignment error"));

              // Act & Assert
              await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
          }

          [Fact]
          public async Task Handle_ShouldPublishEvents_WhenBidderIsCreatedSuccessfully()
          {
              // Arrange

              var bidderDto = new CreateBidderDto
              {
                  UserEmail = "test@example.com",
                  UserPassword = "Test@1234",
                  UserName = "Test User",
                  UserLastName = "Lastname",
                  UserPhone = "1234567890",
                  UserAddress = "Test Address",
                  BidderDni = "12345678",
                  BidderBirthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-18))
              };

              var request = new CreateBidderCommand(bidderDto);

              _bidderRepositoryMongoMock.Setup(repo => repo.GetBidderByEmailAsync(It.IsAny<UserEmail>()))
                  .ReturnsAsync((Bidders)null);

              _keycloakMsServiceMock.Setup(service => service.CreateUserAsync(
                      request.Bidder.UserEmail, request.Bidder.UserPassword, request.Bidder.UserName,
                      request.Bidder.UserLastName, request.Bidder.UserPhone, request.Bidder.UserAddress))
                  .ReturnsAsync("mocked-user-id");

              _keycloakMsServiceMock.Setup(service => service.GetUserByUserName(request.Bidder.UserEmail))
                  .ReturnsAsync(Guid.NewGuid());

              // Act
              var result = await _handler.Handle(request, CancellationToken.None);

              // Assert
              Assert.NotNull(result);
              _eventBusMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetBidderDto>(), "bidderQueue", "BIDDER_CREATED"), Times.Once);
              _eventBusUserMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetUsersDto>(), "userQueue", "USER_CREATED"), Times.Once);
              _eventBusUserRolMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetUserRoleDto>(), "userRoleQueue", "USER_ROLE_CREATED"), Times.Once);
              _eventBusActivityMock.Verify(bus => bus.PublishMessageAsync(It.IsAny<GetActivityHistoryDto>(), "activityHistoryQueue", "ACTIVITY_CREATED"), Times.Once);
          }

      }*/
    }
}
