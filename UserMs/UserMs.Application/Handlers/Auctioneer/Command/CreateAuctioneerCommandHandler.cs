using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthMs.Common.Exceptions;
using FluentValidation;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Application.Validators;
using BCrypt.Net;



namespace UserMs.Application.Handlers.Auctioneer.Command
{
    public class CreateAuctioneerCommandHandler : IRequestHandler<CreateAuctioneerCommand, UserId>
    {
      
            private readonly IAuctioneerRepository _auctioneerRepository;
            private readonly IAuctioneerRepositoryMongo _auctioneerRepositoryMongo;
            private readonly IEventBus<GetAuctioneerDto> _eventBus;
            private readonly IMapper _mapper;
            private readonly IKeycloakService _keycloakMsService;
            private readonly IEventBus<GetUsersDto> _eventBusUser;
            private readonly IUserRepository _usersRepository;
            private readonly IUserRoleRepository _userRoleRepository;
            private readonly IUserRepositoryMongo _usersRepositoryMongo;
            private readonly IUserRoleRepositoryMongo _userRoleRepositoryMongo;
            private readonly IRolesRepository _roleRepository;
            private readonly IActivityHistoryRepository _activityHistoryRepository;
            private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
            private readonly IEventBus<GetUserRoleDto> _eventBusUserRol;

            public CreateAuctioneerCommandHandler(
                IAuctioneerRepository auctioneerRepository,
                IAuctioneerRepositoryMongo auctioneerRepositoryMongo,
                IEventBus<GetAuctioneerDto> eventBus,
                IMapper mapper,
                IKeycloakService keycloakMsService,
                IEventBus<GetUsersDto> eventBusUser,
                IUserRepository usersRepository,
                IUserRoleRepository userRoleRepository,
                IUserRepositoryMongo usersRepositoryMongo,
                IUserRoleRepositoryMongo userRoleRepositoryMongo,
                IRolesRepository roleRepository,
                IActivityHistoryRepository activityHistoryRepository,
                IEventBus<GetActivityHistoryDto> eventBusActivity,
                IEventBus<GetUserRoleDto> eventBusUserRol)
            {
                _auctioneerRepository = auctioneerRepository;
                _auctioneerRepositoryMongo = auctioneerRepositoryMongo;
                _eventBus = eventBus;
                _mapper = mapper;
                _keycloakMsService = keycloakMsService;
                _eventBusUser = eventBusUser;
                _usersRepository = usersRepository;
                _userRoleRepository = userRoleRepository;
                _usersRepositoryMongo = usersRepositoryMongo;
                _userRoleRepositoryMongo = userRoleRepositoryMongo;
                _roleRepository = roleRepository;
                _activityHistoryRepository = activityHistoryRepository;
                _eventBusActivity = eventBusActivity;
                _eventBusUserRol = eventBusUserRol;
            }
        

       
            public async Task<UserId> Handle(CreateAuctioneerCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var userEmailValue = request.Auctioneer.UserEmail;
                    var userPasswordValue = request.Auctioneer.UserPassword;
                    var usersNameValue = request.Auctioneer.UserName;
                    var usersLastNameValue = request.Auctioneer.UserLastName;
                    var usersPhoneValue = request.Auctioneer.UserPhone;
                    var usersAddressValue = request.Auctioneer.UserAddress;
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userPasswordValue);

                    //  Validación de datos de entrada
                    await ValidateAuctioneerRequest(request, cancellationToken);

                    //  Creación y verificación del usuario
                    var Id = await CreateUserInKeycloak(userEmailValue, hashedPassword, usersNameValue, usersLastNameValue, usersPhoneValue, usersAddressValue);

                    //  Creación de entidades
                    var auctioneer = CreateAuctioneerEntity(request.Auctioneer, Id);
                    var users = CreateUserEntity(request.Auctioneer, Id);
                    var userRole = await CreateUserRoleEntity(Id, request.Auctioneer.UserEmail);

                    //  Almacenamiento en repositorios
                    await SaveEntities(users, auctioneer, userRole);

                    //  Publicación de eventos
                    await PublishEvents(auctioneer, users, userRole, Id);

                    return auctioneer.UserId;
                }
                catch (Exception ex)
                {
                    ExceptionHandlerService.HandleException(ex);
                    throw;
                }

            }
            private async Task ValidateAuctioneerRequest(CreateAuctioneerCommand request, CancellationToken cancellationToken)
            {
                var validator = new CreateAuctioneersValidator();
                var validationResult = await validator.ValidateAsync(request.Auctioneer, cancellationToken);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var userExists = await _auctioneerRepositoryMongo.GetAuctioneerByEmailAsync(UserEmail.Create(request.Auctioneer.UserEmail));
                if (userExists != null)
                    throw new UserExistException("El usuario ya existe");
            }

            private async Task<Guid> CreateUserInKeycloak(string userEmail, string userPassword, string userName, string userLastName, string userPhone, string userAddress)
            {
                await _keycloakMsService.CreateUserAsync(userEmail, userPassword, userName, userLastName, userPhone, userAddress);

                var userId = await _keycloakMsService.GetUserByUserName(userEmail);

                if (userId == Guid.Empty)
                    throw new Exception("No se pudo obtener el ID del usuario desde Keycloak.");

                await _keycloakMsService.AssignClientRoleToUser(userId, "Subastador");

                return userId;
            }

            private Auctioneers CreateAuctioneerEntity(CreateAuctioneerDto auctioneerDto, Guid userId)
            {
                return new Auctioneers(
                    UserId.Create(userId),
                    UserEmail.Create(auctioneerDto.UserEmail ?? string.Empty),
                    UserPassword.Create(BCrypt.Net.BCrypt.HashPassword(auctioneerDto.UserPassword) ?? string.Empty),
                    UserName.Create(auctioneerDto.UserName ?? string.Empty),
                    UserPhone.Create(auctioneerDto.UserPhone ?? string.Empty),
                    UserAddress.Create(auctioneerDto.UserAddress ?? string.Empty),
                    UserLastName.Create(auctioneerDto.UserLastName ?? string.Empty),
                    AuctioneerDni.Create(auctioneerDto.AuctioneerDni),
                    AuctioneerBirthday.Create(auctioneerDto.AuctioneerBirthday)
                );
            }
        private Users CreateUserEntity(CreateAuctioneerDto auctioneerDto, Guid userId)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(auctioneerDto.UserPassword);

                return new Users(
                    userId,
                    UserEmail.Create(auctioneerDto.UserEmail),
                    UserPassword.Create(hashedPassword),
                    UserName.Create(auctioneerDto.UserName),
                    UserPhone.Create(auctioneerDto.UserPhone),
                    UserAddress.Create(auctioneerDto.UserAddress),
                    UserLastName.Create(auctioneerDto.UserLastName),
                    Enum.Parse<UsersType>("Subastador"),
                    Enum.Parse<UserAvailable>("Activo"),
                    UserDelete.Create(false)
                );
            }

            private async Task<UserRoles> CreateUserRoleEntity(Guid userId, string userEmail)
            {
                var role = await _roleRepository.GetRolesByNameQuery("Subastador");
                if (role == null)
                    throw new RoleNotFoundException("Role not found");

                var exist = await _userRoleRepositoryMongo.GetRoleByRoleNameAndByUserEmail(role.RoleName.Value, userEmail);
                if (exist != null)
                    throw new UserRoleExistException("Este usuario ya tiene este rol.");

                return new UserRoles(
                    UserRoleId.Create(Guid.NewGuid()),
                    UserId.Create(userId),
                    RoleId.Create(role.RoleId)
                );
            }

            private async Task SaveEntities(Users users, Auctioneers auctioneer, UserRoles userRole)
            {
                await _usersRepository.AddAsync(users);
                await _auctioneerRepository.AddAsync(auctioneer);
                await _userRoleRepository.AddAsync(userRole);
            }

            private async Task PublishEvents(Auctioneers auctioneer, Users users, UserRoles userRole, Guid userId)
            {
                var auctioneerDto = _mapper.Map<GetAuctioneerDto>(auctioneer);
                await _eventBus.PublishMessageAsync(auctioneerDto, "auctioneerQueue", "AUCTIONEER_CREATED");

                var userDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(userDto, "userQueue", "USER_CREATED");

                var userRoleDto = _mapper.Map<GetUserRoleDto>(userRole);
                userRoleDto.UserEmail = users.UserEmail.Value;
                userRoleDto.RoleName = await _roleRepository.GetRolesByNameQuery("Subastador").ContinueWith(t => t.Result.RoleName.Value);

                await _eventBusUserRol.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_CREATED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    userId,
                    "Creación de Subastador",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);

                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
            }

    }
}
